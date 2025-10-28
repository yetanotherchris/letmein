import { useState, useEffect } from 'react';

export default function Countdown({ targetDate, onExpire }) {
  const [timeLeft, setTimeLeft] = useState(null);
  const [hasExpired, setHasExpired] = useState(false);

  useEffect(() => {
    const calculateTimeLeft = () => {
      const now = new Date().getTime();
      const target = new Date(targetDate).getTime();
      const difference = target - now;

      if (difference <= 0) {
        if (onExpire && !hasExpired) {
          setHasExpired(true);
          onExpire();
        }
        return null;
      }

      const days = Math.floor(difference / (1000 * 60 * 60 * 24));
      const hours = Math.floor((difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((difference % (1000 * 60)) / 1000);

      return { days, hours, minutes, seconds };
    };

    setTimeLeft(calculateTimeLeft());

    const timer = setInterval(() => {
      setTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearInterval(timer);
  }, [targetDate, onExpire, hasExpired]);

  if (!timeLeft) {
    return <div className="text-red-500 font-semibold">Expired</div>;
  }

  return (
    <div className="inline-flex items-center space-x-2 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg">
      <span className="font-semibold">Expires in:</span>
      <span>
        {timeLeft.days > 0 && `${timeLeft.days}d `}
        {timeLeft.hours}h {timeLeft.minutes}m {timeLeft.seconds}s
      </span>
    </div>
  );
}
