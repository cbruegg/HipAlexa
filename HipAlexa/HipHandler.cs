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
    public class HipHandler
    {
        private readonly IFactDb _db = new SimpleFactDb();

        [UsedImplicitly]
        public async Task<SkillResponse> Handle(SkillRequest input, ILambdaContext context)
        {
            switch (input.Request)
            {
                case LaunchRequest launchRequest:
                    return await HandleLaunchRequest(launchRequest, context);
                case IntentRequest intentRequest:
                    return await HandleIntentRequest(intentRequest, context);
                case SessionEndedRequest sessionEndedRequest:
                    return await HandleSessionEndedRequest(sessionEndedRequest, context);
            }

            throw new ArgumentException("Unsupported input!");
        }

        private async Task<SkillResponse> HandleLaunchRequest(LaunchRequest launchRequest, ILambdaContext context)
        {
            throw new NotImplementedException();
        }

        private async Task<SkillResponse> HandleSessionEndedRequest(SessionEndedRequest sessionEndedRequest,
            ILambdaContext context)
        {
            throw new NotImplementedException();
        }

        private async Task<SkillResponse> HandleIntentRequest(IntentRequest intentRequest, ILambdaContext context)
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
                    throw new NotImplementedException();
                }
                default:
                    throw new ArgumentException($"Unknown intent {intentRequest.Intent.Name}");
            }
        }
    }
}