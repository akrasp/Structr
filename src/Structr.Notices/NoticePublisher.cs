using Structr.Notices.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Structr.Notices
{
    public class NoticePublisher : INoticePublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, InternalHandler> _cache = new ConcurrentDictionary<Type, InternalHandler>();

        public NoticePublisher(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        public Task PublishAsync(INotice notice, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (notice == null)
                throw new ArgumentNullException(nameof(notice));

            var noticeType = notice.GetType();

            var handler = _cache.GetOrAdd(noticeType,
                type => (InternalHandler)Activator.CreateInstance(typeof(InternalNoticeHandler<>).MakeGenericType(type)));

            return handler.HandleAsync(notice, _serviceProvider, cancellationToken, PublishAsync);
        }

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited. By default the implementation is a foreach and await of each handler
        /// </summary>
        /// <param name="handlers">Enumerable of tasks representing invoking each notice handler</param>
        /// <param name="notice">The notice being published</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual async Task PublishAsync(IEnumerable<Func<INotice, CancellationToken, Task>> handlers,
            INotice notice,
            CancellationToken cancellationToken)
        {
            foreach (var handler in handlers)
            {
                await handler(notice, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
