using System;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using HipAlexa.Mock;
using HipAlexa.Model;
using JetBrains.Annotations;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace HipAlexa
{
    public class HipHandler
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
                case LaunchRequest _:
                    return await HandleLaunchRequestAsync();
                case IntentRequest intentRequest:
                    return await HandleIntentRequestAsync(input, intentRequest, context);
                case SessionEndedRequest _:
                    return await HandleSessionEndedRequestAsync();
            }

            throw new ArgumentException("Unsupported input!");
        }

        private Task<SkillResponse> HandleLaunchRequestAsync()
        {
            var speech =
                new PlainTextOutputSpeech {Text = "Was kann ich für dich tun? Frage nach einem Fakt oder Quiz."};
            return Task.FromResult(ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}));
        }

        private Task<SkillResponse> HandleSessionEndedRequestAsync()
        {
            return Task.FromResult(ResponseBuilder.Tell(new PlainTextOutputSpeech {Text = "Tschüss!"}));
        }

        private async Task<SkillResponse> HandleIntentRequestAsync(SkillRequest request, IntentRequest intentRequest,
            ILambdaContext context)
        {
            switch (intentRequest.Intent.Name)
            {
                case "GiveFactIntent":
                    return await HandleGiveFactIntentRequestAsync(intentRequest, context);
                case "StartQuizIntent":
                    return await HandleStartQuizIntentRequestAsync(request, intentRequest);
                case "AnswerIntent":
                    return await HandleAnswerIntentRequestAsync(request, intentRequest);
                default:
                    throw new ArgumentException($"Unknown intent {intentRequest.Intent.Name}");
            }
        }

        private async Task<SkillResponse> HandleAnswerIntentRequestAsync(SkillRequest request,
            IntentRequest intentRequest)
        {
            if (!State.ContainedIn(request.Session))
            {
                var helpSpeech = new PlainTextOutputSpeech {Text = "Sage: \"Starte Quiz\""};
                return ResponseBuilder.Ask(helpSpeech, new Reprompt {OutputSpeech = helpSpeech});
            }

            var state = State.From(request.Session);
            var quiz = await _db.QuizByIdAsync(state.QuizId);
            var answer = intentRequest.Intent.Slots["answer"].Value;

            if (state.QuestionsPosed < quiz.Stages.Length)
            {
                var session = request.Session;
                var output = "<speak>";
                State nextState;
                if (state.QuestionsPosed > 0)
                {
                    var previousQuestion = quiz.Stages[state.QuestionsPosed - 1];
                    var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                    output += wasLastCorrect.Spoken();
                    nextState = state.Next(wasLastCorrect);
                }
                else
                {
                    output = "<p>Los geht's!</p>";
                    nextState = state;
                }

                var nextQuestion = quiz.Stages[nextState.QuestionsPosed - 1];
                output += $" {nextQuestion.PosedQuestionSsml(wrapInSpeakTags: false)} </speak>";
                var speech = new SsmlOutputSpeech {Ssml = output};
                nextState.WriteTo(session);
                return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
            }
            else
            {
                var previousQuestion = quiz.Stages[state.QuestionsPosed - 1];
                var wasLastCorrect = previousQuestion.IsAnswerCorrect(answer);
                if (wasLastCorrect == IStageExtensions.AnswerResult.UnknownAnswer)
                {
                    var nextQuestion = quiz.Stages[state.QuestionsPosed - 1];
                    var output =
                        $"<speak>{wasLastCorrect.Spoken()} {nextQuestion.PosedQuestionSsml(wrapInSpeakTags: false)}</speak>";
                    var speech = new SsmlOutputSpeech {Ssml = output};
                    return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, request.Session);
                }
                else
                {
                    var correctAnswers = state.CorrectAnswers +
                                         (wasLastCorrect == IStageExtensions.AnswerResult.Correct ? 1 : 0);
                    var output = "<speak>";
                    output += wasLastCorrect.Spoken();
                    output +=
                        $"<p>Insgesamt hast du {correctAnswers} von {quiz.Stages.Length} Fragen richtig beantwortet" +
                        "</p></speak>";
                    return ResponseBuilder.Tell(new SsmlOutputSpeech {Ssml = output});
                }
            }
        }

        private async Task<SkillResponse> HandleStartQuizIntentRequestAsync(SkillRequest request,
            IntentRequest intentRequest)
        {
            var topic = intentRequest.Intent.Slots["topic"]?.Value;
            IQuiz quiz;
            if (topic != null)
            {
                quiz = await _db.RandomQuizAsync(topic) ?? await _db.RandomQuizAsync();
            }
            else
            {
                quiz = await _db.RandomQuizAsync();
            }

            var state = new State(quiz.Id, questionsPosed: 1);
            var session = request.Session;
            state.WriteTo(session);

            var speech = new SsmlOutputSpeech {Ssml = quiz.Stages[0].PosedQuestionSsml()};
            return ResponseBuilder.Ask(speech, new Reprompt {OutputSpeech = speech}, session);
        }

        private async Task<SkillResponse> HandleGiveFactIntentRequestAsync(IntentRequest intentRequest,
            ILambdaContext context)
        {
            var topic = intentRequest.Intent.Slots["topic"]?.Value;
            if (topic != null)
            {
                context.Logger.LogLine($"Topic is {topic}");
                var fact = await _db.RandomFactAsync(topic);
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
                var info = await _db.RandomFactAsync();
                var speech = new PlainTextOutputSpeech
                {
                    Text = $"Hier ein interessanter Fakt über Paderborn: {info.Value}"
                };
                return ResponseBuilder.Tell(speech);
            }
        }
    }
}