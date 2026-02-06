export type Placement = 1 | 2 | 3;

export default function PlayerPlacement(props: { place: Placement, class?: string }) {
  let className = "w-fit rounded-lg p-1 px-2 ";
  if (props.place === 1) className += "bg-[#FFD700]/30";
  else if (props.place === 2) className += "bg-gray-600/60";
  else if (props.place === 3) className += "bg-[#CD7F32]/30";
  else className += "bg-base-300";

  if (props.class) className += ` ${props.class}`;

  return (
    <div class={className}>
      <span># {props.place}</span>
    </div>
  );
}
