interface FatigueMeterProps {
    fatigueLevel: number;
    questsToday: number;
    recommendedMax: number;
}

export default function FatigueMeter({ fatigueLevel, questsToday, recommendedMax }: FatigueMeterProps) {
    const getFatigueColor = () => {
        if (fatigueLevel < 30) return 'from-green-500 to-emerald-500';
        if (fatigueLevel < 70) return 'from-yellow-500 to-orange-500';
        return 'from-red-500 to-pink-500';
    };

    const getFatigueStatus = () => {
        if (fatigueLevel < 30) return 'Fresh';
        if (fatigueLevel < 70) return 'Moderate';
        return 'Exhausted';
    };

    return (
        <div className="glass rounded-xl p-4">
            <h3 className="text-lg font-semibold mb-3">Fatigue Level</h3>
            <div className="mb-3">
                <div className="flex justify-between text-sm mb-2">
                    <span className="text-gray-400">{getFatigueStatus()}</span>
                    <span className="font-semibold">{fatigueLevel.toFixed(0)}%</span>
                </div>
                <div className="h-3 bg-white/5 rounded-full overflow-hidden">
                    <div
                        className={`h-full bg-gradient-to-r ${getFatigueColor()} transition-all duration-500`}
                        style={{ width: `${Math.min(fatigueLevel, 100)}%` }}
                    ></div>
                </div>
            </div>
            <div className="text-xs text-gray-400">
                {questsToday} quests today • Recommended max: {recommendedMax}
            </div>
        </div>
    );
}
