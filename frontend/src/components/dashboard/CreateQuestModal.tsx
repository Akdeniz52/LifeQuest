'use client';

import { useState } from 'react';

interface CreateQuestModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (questData: QuestFormData) => void;
}

export interface QuestFormData {
    title: string;
    description: string;
    questType: string;
    difficulty: string;
    baseXP: number;
    statRewards: { statName: string; value: number }[];
    startImmediately: boolean;
    targetCount: number;
}

export default function CreateQuestModal({ isOpen, onClose, onSubmit }: CreateQuestModalProps) {
    const [formData, setFormData] = useState<QuestFormData>({
        title: '',
        description: '',
        questType: 'Custom',
        difficulty: 'Medium',
        baseXP: 50,
        statRewards: [],
        startImmediately: true,
        targetCount: 1,
    });

    const difficulties = [
        { value: 'Easy', xp: 25, color: 'text-green-400' },
        { value: 'Medium', xp: 50, color: 'text-yellow-400' },
        { value: 'Hard', xp: 100, color: 'text-orange-400' },
        { value: 'Epic', xp: 200, color: 'text-purple-400' },
    ];

    const questTypes = [
        {
            value: 'Daily',
            icon: '🔄',
            description: 'Repeats every day'
        },
        {
            value: 'Weekly',
            icon: '📅',
            description: 'Repeats on selected days'
        },
        {
            value: 'Monthly',
            icon: '📆',
            description: 'Repeats on selected date'
        },
        {
            value: 'Custom',
            icon: '⭐',
            description: 'One-time task'
        }
    ];

    const weekDays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSubmit(formData);
        setFormData({
            title: '',
            description: '',
            questType: 'Daily',
            difficulty: 'Medium',
            baseXP: 50,
            statRewards: [],
            startImmediately: true,
            targetCount: 1,
        });
        onClose();
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
            <div className="glass rounded-2xl p-6 max-w-2xl w-full glow animate-fadeIn max-h-[90vh] overflow-y-auto">
                <div className="flex items-center justify-between mb-6 sticky top-0 bg-black/50 backdrop-blur-sm pb-4 -mt-2">
                    <h2 className="text-2xl font-bold text-gradient">Create New Quest</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-white transition-colors"
                    >
                        ✕
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    {/* Title */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Quest Title
                        </label>
                        <input
                            type="text"
                            value={formData.title}
                            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                            className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                            placeholder="Enter quest title..."
                            required
                        />
                    </div>

                    {/* Description */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Description
                        </label>
                        <textarea
                            value={formData.description}
                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                            className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all resize-none"
                            placeholder="What do you need to accomplish?"
                            rows={3}
                            required
                        />
                    </div>

                    {/* Quest Type */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Quest Type
                        </label>
                        <div className="grid grid-cols-2 gap-3">
                            {questTypes.map((type) => (
                                <button
                                    key={type.value}
                                    type="button"
                                    onClick={() => setFormData({ ...formData, questType: type.value })}
                                    className={`px-4 py-3 rounded-lg font-semibold transition-all ${formData.questType === type.value
                                        ? 'bg-gradient-to-r from-blue-500 to-purple-500 text-white glow'
                                        : 'bg-white/5 text-gray-400 hover:bg-white/10'
                                        }`}
                                >
                                    <div className="text-2xl mb-1">{type.icon}</div>
                                    <div className="text-sm font-bold">{type.value}</div>
                                    <div className="text-xs opacity-70">{type.description}</div>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Difficulty */}
                    <div>
                        <label className="block text-sm font-medium text-gray-300 mb-2">
                            Difficulty
                        </label>
                        <div className="grid grid-cols-4 gap-2">
                            {difficulties.map((diff) => (
                                <button
                                    key={diff.value}
                                    type="button"
                                    onClick={() => setFormData({ ...formData, difficulty: diff.value, baseXP: diff.xp })}
                                    className={`px-4 py-2 rounded-lg font-semibold transition-all ${formData.difficulty === diff.value
                                        ? `bg-gradient-to-r from-blue-500 to-purple-500 text-white glow`
                                        : 'bg-white/5 text-gray-400 hover:bg-white/10'
                                        }`}
                                >
                                    <div className={diff.color}>{diff.value}</div>
                                    <div className="text-xs text-gray-400">{diff.xp} XP</div>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Weekly Day Selector */}
                    {formData.questType === 'Weekly' && (
                        <div>
                            <label className="block text-sm font-medium text-gray-300 mb-2">
                                Select Days
                            </label>
                            <div className="grid grid-cols-7 gap-2">
                                {weekDays.map((day) => (
                                    <button
                                        key={day}
                                        type="button"
                                        className="px-2 py-3 rounded-lg font-semibold transition-all bg-blue-500/20 text-blue-400 hover:bg-blue-500/30 text-xs"
                                    >
                                        {day.slice(0, 3)}
                                    </button>
                                ))}
                            </div>
                            <p className="text-xs text-gray-400 mt-2">Quest will be active on selected days</p>
                        </div>
                    )}

                    {/* Monthly Date Selector */}
                    {formData.questType === 'Monthly' && (
                        <div>
                            <label className="block text-sm font-medium text-gray-300 mb-2">
                                Select Day of Month
                            </label>
                            <input
                                type="number"
                                min="1"
                                max="31"
                                defaultValue="1"
                                className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                                placeholder="Day (1-31)"
                            />
                            <p className="text-xs text-gray-400 mt-2">Quest will be active on this day each month</p>
                        </div>
                    )}

                    {/* Submit Button */}
                    <div className="flex gap-3 pt-4">
                        <button
                            type="button"
                            onClick={onClose}
                            className="flex-1 px-6 py-3 bg-white/5 hover:bg-white/10 rounded-lg font-semibold transition-all"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="flex-1 px-6 py-3 bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 rounded-lg font-semibold transition-all glow"
                        >
                            Create Quest
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
