import { ComponentChildren } from "preact";
import { useSignalRef } from "@preact/signals/utils";

export default function Modal(
  { isOpen, children }: { isOpen: boolean; children: ComponentChildren },
) {
  const dialog = useSignalRef<HTMLDialogElement | null>(null);
  const className = isOpen ? "modal modal-open z-100" : "";

  return (
    <dialog
      ref={dialog}
      open={isOpen}
      class={className}
    >
      {isOpen && children}
    </dialog>
  );
}
