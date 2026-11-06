navigator.serviceWorker.register('service-worker.js', { updateViaCache: 'none' });

window.addEventListener('load', async () => {
    const registration = await navigator.serviceWorker.ready;
    const worker = navigator.serviceWorker.controller || registration.active;
    if (!worker) {
        return;
    }

    const urls = [...new Set(performance.getEntriesByType('resource')
        .map(entry => new URL(entry.name))
        .filter(url => url.origin === location.origin)
        .map(url => url.href))];

    worker.postMessage({ type: 'CACHE_URLS', urls });
});
