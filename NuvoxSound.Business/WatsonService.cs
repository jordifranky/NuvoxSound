using System;
using System.Net; 
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.Assistant.v2;
using IBM.Watson.Assistant.v2.Model;

namespace NuvoxSound.Business
{
    public class WatsonService
    {
        private readonly AssistantService _assistant;
        private readonly string _assistantId = "e2de64cb-807f-433a-9615-90ce468f1418";

        static WatsonService()
        {
            //SOLUCIÓN ESTÁTICA GLOBAL 
           
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public WatsonService()
        {
            IamAuthenticator authenticator = new IamAuthenticator(
                apikey: "YEA7YC8p8TO9s6YbTixKi7YrToK-zl6_5Q_DhLh95Lhr"
            );

            _assistant = new AssistantService("2023-06-15", authenticator);
            _assistant.SetServiceUrl("https://api.au-syd.assistant.watson.cloud.ibm.com");
        }

        public string EnviarMensaje(string mensajeUsuario, string sessionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionId))
                {
                    var session = _assistant.CreateSession(assistantId: _assistantId);
                    sessionId = session?.Result?.SessionId ?? string.Empty;
                }

                var input = new MessageInput() { Text = mensajeUsuario };
                var response = _assistant.Message(
                    assistantId: _assistantId,
                    sessionId: sessionId,
                    input: input
                );

                if (response?.Result?.Output?.Generic != null && response.Result.Output.Generic.Count > 0)
                {
                    string textOutput = response.Result.Output.Generic[0]?.Text;

                    if (textOutput != null && textOutput.Contains("no le entiendo"))
                    {
                        return "[BUSCAR_EN_BD]";
                    }

                    return textOutput ?? "El asistente no devolvió una respuesta de texto.";
                }

                return "El asistente no devolvió una respuesta de texto.";
            }
            catch (Exception ex)
            {
                return "Error de conexión con IBM Watson: " + ex.Message;
            }
        }
    }
}