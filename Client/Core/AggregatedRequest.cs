using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public delegate Task<IReadOnlyDictionary<TRequestParam, TResponse>> AggregatedRequestFunc<TRequestParam, TResponse>(IEnumerable<TRequestParam> parameters);

    public class AggregatedRequest<TRequestParam, TResponse>
    {
        private readonly object _syncRoot = new object();
        private static IReadOnlyDictionary<TRequestParam, TResponse> _empty = new Dictionary<TRequestParam, TResponse>();

        private readonly List<TRequestParam> _parameters;
        private readonly AggregatedRequestFunc<TRequestParam, TResponse> _func;
        private bool _open = true;

        public Task<IReadOnlyDictionary<TRequestParam, TResponse>> Result { get; }

        public int AggregationTimeWindowMs { get; }
        public int MaxDegreeOfAggregation { get; }

        public static AggregatedRequest<TRequestParam, TResponse> Dummy() => new AggregatedRequest<TRequestParam, TResponse>(req => Task.FromResult(_empty), 0, 0);

        public AggregatedRequest(AggregatedRequestFunc<TRequestParam, TResponse> func, int aggregationTimeWindowMs, int maxDegreeOfAggregation)
        {
            _parameters = new List<TRequestParam>();
            _func = func;
            AggregationTimeWindowMs = aggregationTimeWindowMs;
            MaxDegreeOfAggregation = maxDegreeOfAggregation;
            Result = Execute();
        }

        public bool TryAppend(TRequestParam parameter)
        {
            lock (_syncRoot)
            {
                if (_parameters.Count >= MaxDegreeOfAggregation || !_open)
                {
                    // current request is either full or already started executing, can't join
                    return false;
                }

                _parameters.Add(parameter);
                return true;
            }
        }

        public void Append(TRequestParam parameter)
        {
            if (!TryAppend(parameter))
            {
                throw new InvalidOperationException($"Could not append the parameter {parameter} to the request");
            }
        }

        private async Task<IReadOnlyDictionary<TRequestParam, TResponse>> Execute()
        {
            await Task.Delay(AggregationTimeWindowMs);

            lock (_syncRoot)
            {
                _open = false;
            }

            if (_parameters.Count == 0)
                return _empty;

            return await _func(_parameters);
        }
    }
}
