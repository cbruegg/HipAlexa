using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using JetBrains.Annotations;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HipAlexa
{
    public class HipHandler
    {
        private class State
        {
            public int QuizId { get; }
            public int CorrectAnswers { get; }
            public int QuestionsAnswered { get; }

            public State(int quizId, int correctAnswers = 0, int questionsAnswered = 0)
            {
                QuizId = quizId;
                CorrectAnswers = correctAnswers;
                QuestionsAnswered = questionsAnswered;
            }
        }

        private readonly IDb _db = new SimpleDb();

        [UsedImplicitly]
        public async Task<SkillResponse> Handle(SkillRequest input, ILambdaContext context)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.Logger.LogLine($"Request type is {input.Request.GetType()} ({input.GetRequestType()})");

            switch (input.Request)
            {
                case LaunchRequest launchRequest:
                    return await HandleLaunchRequest(input, launchRequest, context);
                case IntentRequest intentRequest:
                    return await HandleIntentRequest(input, intentRequest, context);
                case SessionEndedRequest sessionEndedRequest:
                    return await HandleSessionEndedRequest(input, sessionEndedRequest, context);
            }

            throw new ArgumentException("Unsupported input!");
        }

        private async Task<SkillResponse> HandleLaunchRequest(SkillRequest request, LaunchRequest launchRequest,
            ILambdaContext context)
        {
            return ResponseBuilder.Ask(
                new PlainTextOutputSpeech {Text = "Was kann ich für dich tun? Frage nach einem Fakt oder Quiz."},
                new Reprompt
                {
                    OutputSpeech =
                        new PlainTextOutputSpeech {Text = "Was kann ich für dich tun? Frage nach einem Fakt oder Quiz."}
                });
        }

        private async Task<SkillResponse> HandleSessionEndedRequest(SkillRequest endedRequest,
            SessionEndedRequest sessionEndedRequest,
            ILambdaContext context)
        {
            return ResponseBuilder.Tell(new PlainTextOutputSpeech {Text = "Tschüss!"});
        }

        private async Task<SkillResponse> HandleIntentRequest(SkillRequest request, IntentRequest intentRequest,
            ILambdaContext context)
        {
            switch (intentRequest.Intent.Name)
            {
                case "GiveFactIntent":
                {
                    var topic = intentRequest.Intent.Slots["topic"]?.Value;
                    if (topic != null)
                    {
                        var info = await _db.RandomFact(topic) ?? await _db.RandomFact();
                        var speech = new PlainTextOutputSpeech
                        {
                            Text = $"Zum Thema {topic} habe ich Folgendes gefunden: {info.Value}"
                        };
                        return ResponseBuilder.Tell(speech);
                    }
                    else
                    {
                        var info = await _db.RandomFact();
                        var speech = new PlainTextOutputSpeech
                        {
                            Text = $"Hier ein interessanter Fakt über Paderborn: {info.Value}"
                        };
                        return ResponseBuilder.Tell(speech);
                    }
                }
                case "StartQuizIntent":
                {
                    context.Logger.LogLine($"StartQuizIntent: {intentRequest}");
                    State state;
                    var topic = intentRequest.Intent.Slots["topic"]?.Value;
                    if (topic != null)
                    {
                        state = new State((await _db.RandomQuiz(topic) ?? await _db.RandomQuiz()).Id);
                    }
                    else
                    {
                        state = new State((await _db.RandomQuiz()).Id);
                    }

                    var quiz = await _db.QuizById(state.QuizId);

                    var sessionAttributes = request.Session.Attributes ?? new Dictionary<string, object>();
                    var session = request.Session;
                    session.Attributes = sessionAttributes;
                    sessionAttributes["QuestionsAnswered"] = state.QuestionsAnswered + 1;
                    sessionAttributes["QuizId"] = state.QuizId;
                    sessionAttributes["CorrectAnswers"] = state.CorrectAnswers;

                    var speech = new SsmlOutputSpeech {Ssml = quiz.Stages[0].PosedQuestionSsml()};

                    return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
                }
                case "AnswerIntent":
                {
                    context.Logger.LogLine($"AnswerIntent: {intentRequest}");
                    if (request.Session.Attributes?.ContainsKey("QuizId") != true)
                    {
                        return ResponseBuilder.Ask(new PlainTextOutputSpeech {Text = "Sage: \"Starte Quiz\""},
                            new Reprompt {OutputSpeech = new PlainTextOutputSpeech {Text = "Sage: \"Starte Quiz\""}});
                    }

                    var state = new State(
                        Convert.ToInt32(request.Session.Attributes["QuizId"]),
                        Convert.ToInt32(request.Session.Attributes["CorrectAnswers"]),
                        Convert.ToInt32(request.Session.Attributes["QuestionsAnswered"])
                    );
                    var quiz = await _db.QuizById(state.QuizId);
                    var answer = intentRequest.Intent.Slots["answer"].Value;

                    if (state.QuestionsAnswered < quiz.Stages.Length)
                    {
                        var session = request.Session;
                        session.Attributes["QuestionsAnswered"] = state.QuestionsAnswered + 1;

                        var output = "<speak>";
                        if (state.QuestionsAnswered > 0)
                        {
                            var previousQuestion = quiz.Stages[state.QuestionsAnswered - 1];
                            var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                            output += wasLastCorrect ? "<p><em>Das war richtig!</em></p>" : "<p>Das war leider falsch</p>";
                            if (wasLastCorrect)
                            {
                                session.Attributes["CorrectAnswers"] = state.CorrectAnswers + 1;
                            }
                        }
                        else
                        {
                            output = "<p><em>Los geht's!</em></p>";
                        }

                        var nextQuestion = quiz.Stages[state.QuestionsAnswered];
                        output += $" {nextQuestion.PosedQuestionSsml(wrapInSpeakTags: false)} </speak>";
                        var speech = new SsmlOutputSpeech {Ssml = output};
                        return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
                    }
                    else
                    {
                        var previousQuestion = quiz.Stages[state.QuestionsAnswered - 1];
                        var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                        var correctAnswers = state.CorrectAnswers + (wasLastCorrect ? 1 : 0);
                        var output = wasLastCorrect ? "Das war richtig!" : "Das war leider falsch. ";
                        output +=
                            $"Insgesamt hast du {correctAnswers} von {quiz.Stages.Length} Fragen richtig beantwortet.";
                        return ResponseBuilder.Tell(new PlainTextOutputSpeech {Text = output});
                    }
                }
                default:
                    throw new ArgumentException($"Unknown intent {intentRequest.Intent.Name}");
            }
        }
    }
}