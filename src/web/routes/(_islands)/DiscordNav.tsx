import { useEffect } from "preact/hooks";

export function DiscordNav(
  { clientId, destination }: { clientId: string; destination: string },
) {
  useEffect(() => {
    let cancelled = false;

    (async () => {
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
