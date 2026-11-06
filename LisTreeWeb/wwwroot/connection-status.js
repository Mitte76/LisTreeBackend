window.lisTreeConnection = (() => {
    const subscriptions = new Map();
    let nextId = 0;

    function subscribe(dotNetReference) {
        const id = nextId++;
        let checking = false;

        const updateStatus = async () => {
            if (checking) {
                return;
            }

            checking = true;
            let isOnline = false;
            if (navigator.onLine) {
                try {
                    const url = new URL(`?connection-check=${Date.now()}`, document.baseURI);
                    const response = await fetch(url, { cache: 'no-store' });
                    isOnline = response.ok;
                } catch {
                    isOnline = false;
                }
            }

            checking = false;
            await dotNetReference.invokeMethodAsync('UpdateConnectionStatus', isOnline);
        };

        window.addEventListener('online', updateStatus);
        window.addEventListener('offline', updateStatus);
        const intervalId = window.setInterval(updateStatus, 5000);
        subscriptions.set(id, { updateStatus, intervalId });
        updateStatus();

        return id;
    }

    function unsubscribe(id) {
        const subscription = subscriptions.get(id);
        if (!subscription) {
            return;
        }

        window.removeEventListener('online', subscription.updateStatus);
        window.removeEventListener('offline', subscription.updateStatus);
        window.clearInterval(subscription.intervalId);
        subscriptions.delete(id);
    }

    return { subscribe, unsubscribe };
})();
