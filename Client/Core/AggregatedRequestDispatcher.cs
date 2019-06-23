using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public class AggregatedRequestDispatcher<TRequestParam, TResponse>
    {
        private readonly object _syncRoot = new object();
        private readonly int _aggregationTimeWindow;
        private readonly int _maxDegreeOfAggregation;
        private AggregatedRequest<TRequestParam, TResponse> _current;

        public AggregatedRequestDispatcher(int aggregationTimeWindow, int maxDegreeOfAggregation)
        {
            _current = AggregatedRequest<TRequestParam, TResponse>.Dummy();
            _aggregationTimeWindow = aggregationTimeWindow;
            _maxDegreeOfAggregation = maxDegreeOfAggregation;
        }

        public async Task<TResponse> Dispatch(AggregatedRequestFunc<TRequestParam, TResponse> requestFunc, TRequestParam parameter)
        {
            AggregatedRequest<TRequestParam, TResponse> request;
            lock (_syncRoot)
            {
                if (!_current.TryAppend(parameter))
                {
                    // can't join to current outgoing request, let's make a new one
                    _current = new AggregatedRequest<TRequestParam, TResponse>(requestFunc, _aggregationTimeWindow, _maxDegreeOfAggregation);
                    _current.Append(parameter);
                }
                // grab a reference to the current request
                request = _current;
            }

            var response = await request.Result;
            return response[parameter];
        }
    }
}
