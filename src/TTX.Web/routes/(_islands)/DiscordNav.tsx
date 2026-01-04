import { DiscordSDK } from "@discord/embedded-app-sdk";
import { useSignal, useSignalEffect } from "@preact/signals";

export function DiscordNav(
  { clientId, destination }: { clientId: string; destination: string },
) {
  const sdk = useSignal(new DiscordSDK(clientId));

  useSignalEffect(() => {
    sdk.value.commands.openExternalLink({
      url: destination,
    }).catch(() => {
      sdk.value.commands.openExternalLink({
        url: globalThis.window.location.origin + destination,
      }).catch(console.error);
    });
  });

  return (
    <div>
      Redirecting...
    </div>
  );
}
