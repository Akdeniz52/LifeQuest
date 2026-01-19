interface Skill {
    id: string;
    name: string;
    description: string;
    skillType: string;
    category: string;
    cooldownHours?: number;
    canUnlock?: boolean;
    canUse?: boolean;
}

interface SkillCardProps {
    skill: Skill;
    onUnlock?: (id: string) => void;
    onUse?: (id: string) => void;
    isUnlocked?: boolean;
}

export default function SkillCard({ skill, onUnlock, onUse, isUnlocked }: SkillCardProps) {
    return (
        <div className={`glass glass-hover rounded-xl p-4 border ${isUnlocked ? 'border-white/10' : 'border-gray-500/30 opacity-75'
            }`}>
            <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                        <h4 className="text-lg font-semibold">{skill.name}</h4>
                        {!isUnlocked && <span className="text-xs px-2 py-1 bg-red-500/20 text-red-400 rounded">🔒 Locked</span>}
                    </div>
                    <p className="text-sm text-gray-400 mb-2">{skill.description}</p>
                    <div className="flex gap-2 text-xs flex-wrap">
                        <span className={`px-2 py-1 rounded ${skill.skillType === 'Active'
                                ? 'bg-orange-500/20 text-orange-400'
                                : 'bg-blue-500/20 text-blue-400'
                            }`}>
                            {skill.skillType}
                        </span>
                        <span className="px-2 py-1 bg-purple-500/20 text-purple-400 rounded">
                            {skill.category}
                        </span>
                        {skill.cooldownHours && (
                            <span className="px-2 py-1 bg-gray-500/20 text-gray-400 rounded">
                                {skill.cooldownHours}h cooldown
                            </span>
                        )}
                    </div>
                </div>
            </div>

            {!isUnlocked && !skill.canUnlock && (
                <div className="mt-3 px-4 py-2 bg-yellow-500/20 text-yellow-400 rounded-lg text-center text-sm">
                    Requirements not met
                </div>
            )}

            {!isUnlocked && skill.canUnlock && onUnlock && (
                <button
                    onClick={() => onUnlock(skill.id)}
                    className="w-full mt-3 px-4 py-2 bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600 rounded-lg font-semibold transition-all"
                >
                    Unlock Skill
                </button>
            )}

            {isUnlocked && skill.skillType === 'Active' && skill.canUse && onUse && (
                <button
                    onClick={() => onUse(skill.id)}
                    className="w-full mt-3 px-4 py-2 bg-gradient-to-r from-orange-500 to-red-500 hover:from-orange-600 hover:to-red-600 rounded-lg font-semibold transition-all"
                >
                    Use Skill
                </button>
            )}

            {isUnlocked && skill.skillType === 'Passive' && (
                <div className="mt-3 px-4 py-2 bg-green-500/20 text-green-400 rounded-lg text-center font-semibold">
                    ✓ Active
                </div>
            )}
        </div>
    );
}
