const cacheName = 'listree-development-offline';
const indexUrl = new URL('index.html', self.registration.scope).href;

self.addEventListener('install', event => {
    event.waitUntil((async () => {
        const cache = await caches.open(cacheName);
        await cache.add(indexUrl);
        await self.skipWaiting();
    })());
});

self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        const cacheNames = await caches.keys();
        await Promise.all(cacheNames
            .filter(name => name.startsWith('listree-development-') && name !== cacheName)
            .map(name => caches.delete(name)));
        await self.clients.claim();
    })());
});

self.addEventListener('message', event => {
    if (event.data?.type !== 'CACHE_URLS' || !Array.isArray(event.data.urls)) {
        return;
    }

    event.waitUntil((async () => {
        const cache = await caches.open(cacheName);
        const urls = event.data.urls.filter(url => {
            try {
                return new URL(url).origin === self.location.origin;
            } catch {
                return false;
            }
        });
        await Promise.all(urls.map(async url => {
            try {
                await cache.add(url);
            } catch (error) {
                console.warn('Service worker: Could not cache resource', url, error);
            }
        }));
    })());
});

self.addEventListener('fetch', event => {
    const request = event.request;
    if (request.method !== 'GET' || new URL(request.url).origin !== self.location.origin
        || new URL(request.url).pathname.endsWith('/service-worker.js')) {
        return;
    }

    event.respondWith((async () => {
        const cache = await caches.open(cacheName);
        let response;
        try {
            response = await fetch(request);
        } catch {
            const cachedResponse = await cache.match(request.mode === 'navigate' ? indexUrl : request);
            return cachedResponse ?? Response.error();
        }

        const requestUrl = new URL(request.url);
        if (response.ok && response.type === 'basic' && !requestUrl.searchParams.has('connection-check')) {
            await cache.put(request.mode === 'navigate' ? indexUrl : request, response.clone());
        }
        return response;
    })());
});
