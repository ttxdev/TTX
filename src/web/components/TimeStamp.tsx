import { useEffect, useState } from "preact/hooks";

export default function TimeStamp(props: { date: string }) {
  const date = new Date(props.date);
  const [display, setDisplay] = useState("");

  function updateDisplay() {
    const now = new Date();
    const diffSeconds = Math.floor((Number(now) - Number(date)) / 1000);

    if (diffSeconds < 60) {
      setDisplay(`${diffSeconds} second${diffSeconds !== 1 ? "s" : ""} ago`);
    } else if (diffSeconds < 3600) {
      const minutes = Math.floor(diffSeconds / 60);
      setDisplay(`${minutes} minute${minutes !== 1 ? "s" : ""} ago`);
    } else if (diffSeconds < 86400) {
      const hours = Math.floor(diffSeconds / 3600);
      setDisplay(`${hours} hour${hours !== 1 ? "s" : ""} ago`);
    } else {
      const days = Math.floor(diffSeconds / 86400);
      setDisplay(`${days} day${days !== 1 ? "s" : ""} ago`);
    }
  }

  useEffect(() => {
    updateDisplay();
    const interval = setInterval(updateDisplay, 1000);
    return () => clearInterval(interval);
  }, [date]);

  return <span>{display}</span>;
}
