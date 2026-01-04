import { CreatorDto, TransactionAction } from "@/lib/api.ts";
import Modal from "../../../islands/Modal.tsx";

const FEE_RATE = 0.0;
const BUY_LIMIT = 1000;

export default function OrderModal(
  props: { creator: CreatorDto; show: boolean; type: TransactionAction },
) {
  return (
    <Modal isOpen>
      <span>TODO</span>
    </Modal>
  );
}
