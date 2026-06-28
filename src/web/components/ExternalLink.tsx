import type { ComponentChildren, HTMLAttributeAnchorTarget } from "preact";

export default function ExternalLink(
  props: {
    href: string;
    clientId: string | null;
    target?: HTMLAttributeAnchorTarget;
    class?: string;
    ariaLabel?: string;
    children: ComponentChildren;
  },
) {
  const className = props.class
    ? props.class + " cursor-pointer"
    : "cursor-pointer";
  let href = props.href;

  if (!props.clientId) {
    href = `/external?to=${encodeURIComponent(href)}`;
  }

  return (
    <a
      href={href}
      target={props.target ?? "_blank"}
      class={className}
      aria-label={props.ariaLabel}
    >
      {props.children}
    </a>
  );
}
