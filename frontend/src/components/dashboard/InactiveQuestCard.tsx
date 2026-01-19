'use client';

interface QuestDefinition {
    id: string;
    title: string;
    description: string;
    questType: string;
    baseXP: number;
    isActive: boolean;
    autoAssign: boolean;
    recurrenceType?: string;
}

interface InactiveQuestCardProps {
    quest: QuestDefinition;
}

export default function InactiveQuestCard({ quest }: InactiveQuestCardProps) {
    const getScheduleInfo = () => {
        if (quest.questType === 'Daily') {
            return {
                icon: '🔄',
                text: 'Assigned daily at midnight',
                color: 'text-blue-400',
                bg: 'bg-blue-500/20',
            };
        }
        if (quest.questType === 'Weekly') {
            return {
                icon: '📅',
                text: 'Assigned every Monday',
                color: 'text-purple-400',
                bg: 'bg-purple-500/20',
            };
        }
        return {
            icon: '✨',
            text: 'Manual assignment required',
            color: 'text-gray-400',
            bg: 'bg-gray-500/20',
        };
    };

    const scheduleInfo = getScheduleInfo();

    return (
        <div className="glass glass-hover rounded-xl p-4 border border-white/10 opacity-75">
            <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                        <h3 className="text-lg font-semibold">{quest.title}</h3>
                        <span className="text-xs px-2 py-1 bg-gray-500/20 text-gray-400 rounded">
                            Scheduled
                        </span>
                    </div>
                    <p className="text-gray-400 text-sm mb-2">{quest.description}</p>
                    <div className="flex gap-3 text-xs">
                        <span className="px-2 py-1 bg-blue-500/20 text-blue-400 rounded">
                            {quest.questType}
                        </span>
                        <span className="px-2 py-1 bg-purple-500/20 text-purple-400 rounded">
                            +{quest.baseXP} XP
                        </span>
                    </div>
                </div>
            </div>

            <div className={`mt-3 px-4 py-2 ${scheduleInfo.bg} ${scheduleInfo.color} rounded-lg text-center text-sm flex items-center justify-center gap-2`}>
                <span>{scheduleInfo.icon}</span>
                <span>{scheduleInfo.text}</span>
            </div>
        </div>
    );
}
