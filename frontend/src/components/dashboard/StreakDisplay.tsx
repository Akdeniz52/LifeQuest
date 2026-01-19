interface StreakDisplayProps {
    dailyStreak: number;
    weeklyStreak: number;
}

export default function StreakDisplay({ dailyStreak, weeklyStreak }: StreakDisplayProps) {
    return (
        <div className="glass rounded-xl p-4">
            <h3 className="text-lg font-semibold mb-3 text-gradient">Streaks</h3>
            <div className="grid grid-cols-2 gap-4">
                <div className="text-center">
                    <div className="text-3xl font-bold text-blue-400 mb-1">🔥 {dailyStreak}</div>
                    <div className="text-sm text-gray-400">Daily Streak</div>
                </div>
                <div className="text-center">
                    <div className="text-3xl font-bold text-purple-400 mb-1">⭐ {weeklyStreak}</div>
                    <div className="text-sm text-gray-400">Weekly Streak</div>
                </div>
            </div>
        </div>
    );
}
