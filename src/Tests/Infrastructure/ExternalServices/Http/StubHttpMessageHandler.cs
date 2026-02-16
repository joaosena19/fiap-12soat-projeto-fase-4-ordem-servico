using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Infrastructure.ExternalServices.Http
{
    /// <summary>
    /// Handler HTTP simulado para testes unitários que permite configurar respostas esperadas por rota.
    /// </summary>
    public class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, Queue<Func<HttpRequestMessage, Task<HttpResponseMessage>>>> _respostasPorRota = new();
        private readonly List<HttpRequestMessage> _requests = new();

        public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

        /// <summary>
        /// Configura uma resposta esperada para uma rota específica (método HTTP + path).
        /// </summary>
        public RouteSetupBuilder ParaRota(string metodoHttp, string pathAndQuery)
        {
            var chave = CriarChaveRota(metodoHttp, pathAndQuery);
            return new RouteSetupBuilder(this, chave);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requests.Add(request);

            var chave = CriarChaveRota(request.Method.Method, request.RequestUri?.PathAndQuery ?? "/");

            if (!_respostasPorRota.TryGetValue(chave, out var respostas) || respostas.Count == 0)
                throw new InvalidOperationException($"Requisição não esperada: {request.Method} {request.RequestUri?.PathAndQuery}. Rotas configuradas: {string.Join(", ", _respostasPorRota.Keys)}");

            var fabricaResposta = respostas.Dequeue();
            return await fabricaResposta(request);
        }

        private static string CriarChaveRota(string metodoHttp, string pathAndQuery) => $"{metodoHttp.ToUpperInvariant()} {pathAndQuery}";

        internal void AdicionarResposta(string chaveRota, Func<HttpRequestMessage, Task<HttpResponseMessage>> fabricaResposta)
        {
            if (!_respostasPorRota.ContainsKey(chaveRota))
                _respostasPorRota[chaveRota] = new Queue<Func<HttpRequestMessage, Task<HttpResponseMessage>>>();

            _respostasPorRota[chaveRota].Enqueue(fabricaResposta);
        }
    }

    /// <summary>
    /// Builder fluente para configurar respostas de uma rota no StubHttpMessageHandler.
    /// </summary>
    public class RouteSetupBuilder
    {
        private readonly StubHttpMessageHandler _handler;
        private readonly string _chaveRota;

        internal RouteSetupBuilder(StubHttpMessageHandler handler, string chaveRota)
        {
            _handler = handler;
            _chaveRota = chaveRota;
        }

        /// <summary>
        /// Configura a resposta para retornar um status HTTP e um corpo JSON.
        /// </summary>
        public RouteSetupBuilder Retornar(HttpStatusCode statusCode, object? corpo = null)
        {
            _handler.AdicionarResposta(_chaveRota, _ =>
            {
                var response = new HttpResponseMessage(statusCode);
                
                if (corpo != null)
                {
                    var json = JsonSerializer.Serialize(corpo);
                    response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                
                return Task.FromResult(response);
            });
            
            return this;
        }

        /// <summary>
        /// Configura a resposta para lançar uma exceção.
        /// </summary>
        public RouteSetupBuilder Lancar<TException>() where TException : Exception, new()
        {
            _handler.AdicionarResposta(_chaveRota, _ => throw new TException());
            return this;
        }

        /// <summary>
        /// Configura a resposta para lançar uma exceção com mensagem personalizada.
        /// </summary>
        public RouteSetupBuilder Lancar<TException>(string mensagem) where TException : Exception
        {
            _handler.AdicionarResposta(_chaveRota, _ => throw (TException)Activator.CreateInstance(typeof(TException), mensagem)!);
            return this;
        }

        /// <summary>
        /// Configura a resposta para simular um timeout.
        /// </summary>
        public RouteSetupBuilder SimularTimeout()
        {
            _handler.AdicionarResposta(_chaveRota, _ =>
            {
                var timeoutException = new TimeoutException("A operação expirou.");
                throw new TaskCanceledException("A requisição foi cancelada devido a timeout.", timeoutException);
            });
            return this;
        }
    }
}
