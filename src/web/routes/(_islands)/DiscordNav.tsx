import { useEffect } from "preact/hooks";

export function DiscordNav(
  { clientId, destination }: { clientId: string; destination: string },
) {
  useEffect(() => {
    let cancelled = false;

    (async () => {
      // Load the Discord SDK on demand. It's ~150KB and only needed at runtime
      // inside the Discord activity, so a dynamic import keeps it out of the
      // island's initial chunk — this page hydrates instantly and fetches the
      // SDK in the background.
      const { DiscordSDK } = await import("@discord/embedded-app-sdk");
      if (cancelled) {
        return;
      }

      const sdk = new DiscordSDK(clientId);
      try {
        await sdk.commands.openExternalLink({ url: destination });
      } catch {
        try {
          await sdk.commands.openExternalLink({
            url: globalThis.window.location.origin + destination,
          });
        } catch (err) {
          console.error(err);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [clientId, destination]);

  return (
    <div>
      Redirecting...
    </div>
  );
}
