using System;
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
    public partial class HipHandler
    {
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
                        var fact = await _db.RandomFact(topic);
                        if (fact == null)
                        {
                            return ResponseBuilder.Tell($"Zum Thema {topic} habe ich leider nichts gefunden.");
                        }
                        var speech = new PlainTextOutputSpeech
                        {
                            Text = $"Zum Thema {topic} habe ich Folgendes gefunden: {fact.Value}"
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
                    var topic = intentRequest.Intent.Slots["topic"]?.Value;
                    IQuiz quiz;
                    if (topic != null)
                    {
                        quiz = await _db.RandomQuiz(topic) ?? await _db.RandomQuiz();
                    }
                    else
                    {
                        quiz = await _db.RandomQuiz();
                    }

                    var state = new State(quiz.Id, questionsPosed: 1);

                    var session = request.Session;
                    state.WriteTo(session);

                    var speech = new SsmlOutputSpeech {Ssml = quiz.Stages[0].PosedQuestionSsml()};

                    return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
                }
                case "AnswerIntent":
                {
                    context.Logger.LogLine($"AnswerIntent: {intentRequest}");
                    if (!State.ContainedIn(request.Session))
                    {
                        return ResponseBuilder.Ask(new PlainTextOutputSpeech {Text = "Sage: \"Starte Quiz\""},
                            new Reprompt {OutputSpeech = new PlainTextOutputSpeech {Text = "Sage: \"Starte Quiz\""}});
                    }

                    var state = State.From(request.Session);
                    var quiz = await _db.QuizById(state.QuizId);
                    var answer = intentRequest.Intent.Slots["answer"].Value;
                    context.Logger.LogLine(
                        $"Got answer {answer} to question {quiz.Stages[state.QuestionsPosed - 1].Question}");

                    if (state.QuestionsPosed < quiz.Stages.Length)
                    {
                        var session = request.Session;
                        var output = "<speak>";
                        State nextState;
                        if (state.QuestionsPosed > 0)
                        {
                            var previousQuestion = quiz.Stages[state.QuestionsPosed - 1];
                            var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                            output += wasLastCorrect
                                ? "<p>Das war richtig!</p>"
                                : "<p>Das war leider falsch</p>";
                            nextState = state.Next(wasLastCorrect);
                        }
                        else
                        {
                            output = "<p>Los geht's!</p>";
                            nextState = state;
                        }

                        var nextQuestion = quiz.Stages[state.QuestionsPosed];
                        output += $" {nextQuestion.PosedQuestionSsml(wrapInSpeakTags: false)} </speak>";
                        var speech = new SsmlOutputSpeech {Ssml = output};
                        nextState.WriteTo(session);
                        return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
                    }
                    else
                    {
                        var previousQuestion = quiz.Stages[state.QuestionsPosed - 1];
                        var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                        var correctAnswers = state.CorrectAnswers + (wasLastCorrect ? 1 : 0);
                        var output = "<speak><p>";
                        output += wasLastCorrect ? "Das war richtig!" : "Das war leider falsch. ";
                        output += "</p> ";
                        output +=
                            $"<p>Insgesamt hast du {correctAnswers} von {quiz.Stages.Length} Fragen richtig beantwortet" +
                            "</p></speak>";
                        return ResponseBuilder.Tell(new SsmlOutputSpeech {Ssml = output});
                    }
                }
                default:
                    throw new ArgumentException($"Unknown intent {intentRequest.Intent.Name}");
            }
        }
    }
}