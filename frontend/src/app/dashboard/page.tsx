'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import StreakDisplay from '@/components/dashboard/StreakDisplay';
import FatigueMeter from '@/components/dashboard/FatigueMeter';
import SkillCard from '@/components/dashboard/SkillCard';
import CreateQuestModal, { QuestFormData } from '@/components/dashboard/CreateQuestModal';
import Notification from '@/components/dashboard/Notification';
import InactiveQuestCard from '@/components/dashboard/InactiveQuestCard';
import JournalTab from '@/components/dashboard/JournalTab';

interface CharacterProfile {
    name: string;
    level: number;
    currentXP: number;
    totalXP: number;
    xpForNextLevel: number;
    availableStatPoints: number;
}

interface Stat {
    id: string;
    name: string;
    currentValue: number;
    maxValue: number;
    category: string;
    description: string;
    isLocked: boolean;
}

interface Quest {
    id: string;
    title: string;
    description: string;
    questType: string;
    baseXP: number;
    status: string;
    deadline: string;
    completionCount?: number;
}

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

type TabType = 'dailies' | 'habits' | 'journal';

export default function DashboardPage() {
    const router = useRouter();
    const [activeTab, setActiveTab] = useState<TabType>('dailies');
    const [profile, setProfile] = useState<CharacterProfile | null>(null);
    const [stats, setStats] = useState<Stat[]>([]);
    const [quests, setQuests] = useState<Quest[]>([]);
    const [availableSkills, setAvailableSkills] = useState<Skill[]>([]);
    const [unlockedSkills, setUnlockedSkills] = useState<Skill[]>([]);
    const [streaks, setStreaks] = useState({ daily: 0, weekly: 0 });
    const [fatigue, setFatigue] = useState({ level: 0, questsToday: 0, recommendedMax: 10 });
    const [loading, setLoading] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [notification, setNotification] = useState<{ message: string; type: 'success' | 'error' | 'info' } | null>(null);
    const [questHistory, setQuestHistory] = useState<Quest[]>([]);
    const [inactiveQuests, setInactiveQuests] = useState<any[]>([]);
    const [journalEntries, setJournalEntries] = useState<any[]>([]);

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (!token) {
            router.push('/');
            return;
        }

        loadData(token);
    }, []);

    const loadData = async (token: string) => {
        try {
            console.log('Loading dashboard data...');
            const [profileData, statsData, questsData] = await Promise.all([
                api.character.getProfile(token),
                api.character.getStats(token),
                api.quests.getToday(token),
            ]);

            console.log('Loaded quests:', questsData);
            console.log('Quest count:', questsData.length);

            setProfile(profileData);
            setStats(statsData);
            setQuests(questsData);

            // Load additional data
            loadStreaks(token);
            loadFatigue(token);
            loadSkills(token);
            loadQuestHistory(token);
            loadInactiveQuests(token);
            loadJournalEntries(token);
        } catch (error) {
            console.error('Failed to load data:', error);
            router.push('/');
        } finally {
            setLoading(false);
        }
    };

    const loadQuestHistory = async (token: string) => {
        try {
            const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/quests/active`, {
                headers: { 'Authorization': `Bearer ${token}` },
            });
            if (response.ok) {
                const allQuests = await response.json();
                setQuestHistory(allQuests.filter((q: Quest) => q.status === 'Completed'));
            }
        } catch (error) {
            console.error('Failed to load quest history:', error);
        }
    };

    const loadInactiveQuests = async (token: string) => {
        try {
            const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/quests/definitions`, {
                headers: { 'Authorization': `Bearer ${token}` },
            });
            if (response.ok) {
                const definitions = await response.json();
                setInactiveQuests(definitions.filter((q: any) => q.questType === 'Daily' || q.questType === 'Weekly'));
            }
        } catch (error) {
            console.error('Failed to load inactive quests:', error);
        }
    };

    const loadJournalEntries = async (token: string) => {
        try {
            const entries = await api.journal.getEntries(token);
            setJournalEntries(entries);
        } catch (error) {
            console.error('Failed to load journal entries:', error);
        }
    };

    const loadStreaks = async (token: string) => {
        try {
            const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/character/streaks`, {
                headers: { 'Authorization': `Bearer ${token}` },
            });
            if (response.ok) {
                const data = await response.json();
                setStreaks({
                    daily: data.dailyStreak?.currentStreak || 0,
                    weekly: data.weeklyStreak?.currentStreak || 0,
                });
            }
        } catch (error) {
            console.error('Failed to load streaks:', error);
        }
    };

    const loadFatigue = async (token: string) => {
        try {
            const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/character/fatigue`, {
                headers: { 'Authorization': `Bearer ${token}` },
            });
            if (response.ok) {
                const data = await response.json();
                setFatigue({
                    level: data.currentFatigue || 0,
                    questsToday: data.questsCompletedToday || 0,
                    recommendedMax: data.recommendedMaxQuests || 10,
                });
            }
        } catch (error) {
            console.error('Failed to load fatigue:', error);
        }
    };

    const loadSkills = async (token: string) => {
        try {
            const [available, unlocked] = await Promise.all([
                api.skills.getAvailable(token),
                api.skills.getUnlocked(token),
            ]);
            setAvailableSkills(available);
            setUnlockedSkills(unlocked);
        } catch (error) {
            console.error('Failed to load skills:', error);
        }
    };

    const handleCompleteQuest = async (questId: string) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
            await api.quests.complete(token, questId);

            // Reload quests to get updated status
            const questsData = await api.quests.getToday(token);
            console.log('Quests after completion:', questsData);
            setQuests(questsData);

            // Reload profile for XP update
            const profileData = await api.character.getProfile(token);
            setProfile(profileData);

            // Update fatigue counter
            const completedToday = questsData.filter((q: Quest) => q.status === 'Completed').length;
            console.log('Completed quests today:', completedToday, 'Total quests:', questsData.length);

            setFatigue(prev => {
                console.log('Updating fatigue from', prev.questsToday, 'to', completedToday);
                return {
                    ...prev,
                    questsToday: completedToday
                };
            });

            // Reload quest history
            loadQuestHistory(token);

            showNotification('Quest completed! 🎉', 'success');
        } catch (error) {
            console.error('Failed to complete quest:', error);
            showNotification('Failed to complete quest', 'error');
        }
    };

    const handleFailQuest = async (questId: string) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        if (!confirm('Are you sure you want to fail this quest? This cannot be undone.')) return;

        try {
            await api.quests.fail(token, questId);
            // Reload quests and history
            const questsData = await api.quests.getToday(token);
            setQuests(questsData);
            loadQuestHistory(token); // To update the list of completed/failed quests
            showNotification('Quest failed.', 'info');
        } catch (error) {
            console.error('Failed to fail quest:', error);
            showNotification('Failed to fail quest', 'error');
        }
    };

    const handleDeleteQuest = async (questId: string) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        if (!confirm('Are you sure you want to delete this quest?')) return;

        try {
            await api.quests.delete(token, questId);
            await loadInactiveQuests(token);
            showNotification('Quest deleted successfully', 'success');
        } catch (error) {
            console.error('Failed to delete quest:', error);
            showNotification('Failed to delete quest', 'error');
        }
    };

    const handleUnlockSkill = async (skillId: string) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
            await api.skills.unlock(token, skillId);
            loadSkills(token);
            showNotification('Skill unlocked!', 'success');
        } catch (error) {
            console.error('Failed to unlock skill:', error);
            showNotification('Failed to unlock skill', 'error');
        }
    };

    const handleUseSkill = async (skillId: string) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
            await api.skills.use(token, skillId);
            showNotification('Skill activated successfully!', 'success');
            loadSkills(token);
            loadData(token);
        } catch (error) {
            console.error('Failed to use skill:', error);
            showNotification('Failed to use skill', 'error');
        }
    };

    const handleCreateQuest = async (questData: QuestFormData) => {
        const token = localStorage.getItem('token');
        if (!token) return;

        try {
            console.log('Creating quest with data:', questData);

            // Create quest definition
            // Convert statRewards to StatEffects format
            const statEffects = questData.statRewards.map(reward => ({
                statDefinitionId: stats.find(s => s.name === reward.statName)?.id || '',
                effectMultiplier: reward.value
            })).filter(effect => effect.statDefinitionId);

            const createdQuest = await api.quests.create(token, {
                title: questData.title,
                description: questData.description,
                questType: questData.questType,
                baseXP: questData.baseXP,
                statEffects: statEffects,
                isMandatory: false,
                difficultyMultiplier: 1.0,
            });

            console.log('Quest created:', createdQuest);

            // Auto-assign if startImmediately is checked
            if (questData.startImmediately) {
                console.log('Auto-assigning quest:', createdQuest.id);
                try {
                    const assignResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/quests/assign/${createdQuest.id}`, {
                        method: 'POST',
                        headers: { 'Authorization': `Bearer ${token}` },
                    });

                    if (!assignResponse.ok) {
                        const errorText = await assignResponse.text();
                        console.error('Assign failed:', assignResponse.status, errorText);
                    } else {
                        const assignData = await assignResponse.json();
                        console.log('Quest assigned:', assignData);
                    }
                } catch (assignError) {
                    console.error('Failed to auto-assign quest:', assignError);
                }
            }

            showNotification('Quest created successfully!', 'success');
            await loadData(token);
        } catch (error) {
            console.error('Failed to create quest:', error);
            showNotification('Failed to create quest', 'error');
        }
    };

    const showNotification = (message: string, type: 'success' | 'error' | 'info') => {
        setNotification({ message, type });
        setTimeout(() => setNotification(null), 3000);
    };

    if (loading) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-black">
                <div className="text-2xl text-gradient animate-pulse">Loading System...</div>
            </div>
        );
    }

    const xpPercentage = profile ? (profile.currentXP / profile.xpForNextLevel) * 100 : 0;

    const tabs = [
        { id: 'dailies' as TabType, label: 'Dailies', icon: '📅' },
        { id: 'habits' as TabType, label: 'Habits', icon: '🔄' },
        { id: 'journal' as TabType, label: 'Journal', icon: '📔' },
    ];

    return (
        <div className="min-h-screen bg-gradient-to-br from-black via-gray-900 to-black p-6">
            {/* Header */}
            <div className="max-w-7xl mx-auto mb-8">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-4xl font-bold text-gradient mb-2">[ SYSTEM ]</h1>
                        <p className="text-gray-400">Welcome back, {profile?.name}</p>
                    </div>
                    <button
                        onClick={() => {
                            localStorage.clear();
                            router.push('/');
                        }}
                        className="px-4 py-2 bg-red-500/20 hover:bg-red-500/30 border border-red-500/50 rounded-lg transition-all"
                    >
                        Logout
                    </button>
                </div>
            </div>

            {/* Navigation Tabs */}
            <div className="max-w-7xl mx-auto mb-6">
                <div className="glass rounded-xl p-2 flex gap-2">
                    {tabs.map((tab) => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`flex-1 px-4 py-3 rounded-lg font-semibold transition-all ${activeTab === tab.id
                                ? 'bg-gradient-to-r from-blue-500 to-purple-500 text-white glow'
                                : 'bg-white/5 text-gray-400 hover:bg-white/10'
                                }`}
                        >
                            <span className="mr-2">{tab.icon}</span>
                            {tab.label}
                        </button>
                    ))}
                </div>
            </div>

            <div className="max-w-7xl mx-auto">
                {/* Dailies Tab */}
                {activeTab === 'dailies' && (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        {/* Character Card */}
                        <div className="lg:col-span-1">
                            <div className="glass rounded-2xl p-6 glow mb-6">
                                <div className="text-center mb-6">
                                    <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gradient-to-br from-blue-500 to-purple-500 flex items-center justify-center text-4xl font-bold glow">
                                        {profile?.level}
                                    </div>
                                    <h2 className="text-2xl font-bold text-gradient mb-1">{profile?.name}</h2>
                                    <p className="text-gray-400">Level {profile?.level}</p>
                                </div>

                                {/* XP Bar */}
                                <div className="mb-6">
                                    <div className="flex justify-between text-sm mb-2">
                                        <span className="text-gray-400">Experience</span>
                                        <span className="text-blue-400 font-semibold">
                                            {profile?.currentXP} / {profile?.xpForNextLevel} XP
                                        </span>
                                    </div>
                                    <div className="h-3 bg-white/5 rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-blue-500 to-purple-500 transition-all duration-500 relative"
                                            style={{ width: `${xpPercentage}%` }}
                                        >
                                            <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/30 to-transparent animate-shimmer"></div>
                                        </div>
                                    </div>
                                </div>

                                {/* Stats */}
                                <div className="space-y-3">
                                    <div className="flex justify-between items-center mb-3">
                                        <h3 className="text-lg font-semibold text-gray-300">Your Stats</h3>
                                        {profile && profile.availableStatPoints > 0 && (
                                            <div className="px-3 py-1 bg-gradient-to-r from-yellow-500/20 to-orange-500/20 border border-yellow-500/30 rounded-lg">
                                                <span className="text-yellow-400 font-bold">
                                                    ⭐ {profile.availableStatPoints} {profile.availableStatPoints === 1 ? 'Point' : 'Points'} Available
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                    {stats.filter(s => !s.isLocked).slice(0, 6).map((stat) => (
                                        <div key={stat.id} className="space-y-1">
                                            <div className="flex justify-between items-center gap-2">
                                                <div className="flex-1">
                                                    <div className="flex justify-between items-center">
                                                        <span className="text-white font-bold text-base">{stat.name}</span>
                                                        <span className="text-blue-400 font-bold text-lg">
                                                            {stat.currentValue.toFixed(2)}
                                                        </span>
                                                    </div>
                                                </div>
                                                {profile && profile.availableStatPoints > 0 && (
                                                    <button
                                                        onClick={async () => {
                                                            const token = localStorage.getItem('token');
                                                            if (!token) return;
                                                            try {
                                                                const updatedProfile = await api.character.distributeStat(token, stat.id);
                                                                setProfile(updatedProfile);
                                                                const updatedStats = await api.character.getStats(token);
                                                                setStats(updatedStats);
                                                                showNotification(`+1 ${stat.name}!`, 'success');
                                                            } catch (error) {
                                                                console.error('Failed to distribute stat:', error);
                                                                showNotification('Failed to distribute stat point', 'error');
                                                            }
                                                        }}
                                                        className="px-2 py-1 bg-green-500/20 hover:bg-green-500/30 text-green-400 rounded transition-all text-sm font-bold"
                                                        title="Add 1 point"
                                                    >
                                                        +
                                                    </button>
                                                )}
                                            </div>
                                            <div className="h-2 bg-white/5 rounded-full overflow-hidden">
                                                <div
                                                    className="h-full bg-gradient-to-r from-blue-500 to-purple-500"
                                                    style={{ width: `${(stat.currentValue / stat.maxValue) * 100}%` }}
                                                ></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>



                            {/* Fatigue */}
                            <div className="mt-6">
                                <FatigueMeter
                                    fatigueLevel={fatigue.level}
                                    questsToday={fatigue.questsToday}
                                    recommendedMax={fatigue.recommendedMax}
                                />
                            </div>
                        </div>

                        {/* Today's Quests */}
                        <div className="lg:col-span-2">
                            <div className="glass rounded-2xl p-6">
                                {/* Quest List - Habitica Style */}
                                <div className="flex items-center justify-between mb-6">
                                    <h2 className="text-2xl font-bold">Daily Tasks</h2>
                                    <button
                                        onClick={() => setShowCreateModal(true)}
                                        className="px-4 py-2 bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 rounded-lg font-semibold transition-all"
                                    >
                                        + New Task
                                    </button>
                                </div>

                                {quests.length === 0 ? (
                                    <div className="text-center py-12 text-gray-400">
                                        <p className="text-lg mb-2">No tasks yet</p>
                                        <p className="text-sm">Create your first task to get started!</p>
                                    </div>
                                ) : (
                                    <div className="space-y-3">
                                        {quests.map((quest) => (
                                            <div
                                                key={quest.id}
                                                className={`glass glass-hover rounded-xl p-4 border transition-all ${quest.status === 'Completed'
                                                    ? 'border-green-500/30 bg-green-500/5'
                                                    : 'border-white/10'
                                                    }`}
                                            >
                                                <div className="flex items-center gap-4">
                                                    {/* Checkbox */}
                                                    <button
                                                        onClick={() => {
                                                            if (quest.status === 'Pending') {
                                                                handleCompleteQuest(quest.id);
                                                            }
                                                        }}
                                                        className={`flex-shrink-0 w-6 h-6 rounded border-2 flex items-center justify-center transition-all ${quest.status === 'Completed'
                                                            ? 'bg-green-500 border-green-500'
                                                            : 'border-gray-400 hover:border-blue-400'
                                                            }`}
                                                    >
                                                        {quest.status === 'Completed' && (
                                                            <span className="text-white text-sm">✓</span>
                                                        )}
                                                    </button>

                                                    {/* Quest Info */}
                                                    <div className="flex-1">
                                                        <h3 className={`text-lg font-semibold mb-1 ${quest.status === 'Completed' ? 'line-through text-gray-400' : ''
                                                            }`}>
                                                            {quest.title}
                                                        </h3>
                                                        <p className="text-gray-400 text-sm mb-2">{quest.description}</p>
                                                        <div className="flex gap-2 text-xs">
                                                            <span className="px-2 py-1 bg-purple-500/20 text-purple-400 rounded">
                                                                +{quest.baseXP} XP
                                                            </span>
                                                            <span className="px-2 py-1 bg-blue-500/20 text-blue-400 rounded">
                                                                {quest.questType}
                                                            </span>
                                                        </div>
                                                    </div>

                                                    {/* Streak Counter */}
                                                    <div className="flex-shrink-0 text-right">
                                                        <div className="text-2xl font-bold text-orange-400">🔥</div>
                                                        <div className="text-sm text-gray-400">{quest.completionCount || 0} day{(quest.completionCount || 0) !== 1 ? 's' : ''}</div>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                )}

                {/* Habits Tab */}
                {activeTab === 'habits' && (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        {/* Character Card - Same as Dailies */}
                        <div className="lg:col-span-1">
                            <div className="glass rounded-2xl p-6 glow mb-6">
                                <div className="text-center mb-6">
                                    <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gradient-to-br from-blue-500 to-purple-500 flex items-center justify-center text-4xl font-bold glow">
                                        {profile?.level}
                                    </div>
                                    <h2 className="text-2xl font-bold text-gradient mb-1">{profile?.name}</h2>
                                    <p className="text-gray-400">Level {profile?.level}</p>
                                </div>

                                {/* XP Bar */}
                                <div className="mb-6">
                                    <div className="flex justify-between text-sm mb-2">
                                        <span className="text-gray-400">Experience</span>
                                        <span className="text-blue-400 font-semibold">
                                            {profile?.currentXP} / {profile?.xpForNextLevel} XP
                                        </span>
                                    </div>
                                    <div className="h-3 bg-white/5 rounded-full overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-blue-500 to-purple-500 transition-all duration-500 glow"
                                            style={{ width: `${xpPercentage}%` }}
                                        ></div>
                                    </div>
                                </div>

                                {/* Stats */}
                                <div className="space-y-3">
                                    <div className="flex justify-between items-center mb-3">
                                        <h3 className="text-lg font-semibold text-gray-300">Your Stats</h3>
                                        {profile && profile.availableStatPoints > 0 && (
                                            <div className="px-3 py-1 bg-gradient-to-r from-yellow-500/20 to-orange-500/20 border border-yellow-500/30 rounded-lg">
                                                <span className="text-yellow-400 font-bold">
                                                    ⭐ {profile.availableStatPoints} {profile.availableStatPoints === 1 ? 'Point' : 'Points'} Available
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                    {stats.filter(s => !s.isLocked).slice(0, 6).map((stat) => (
                                        <div key={stat.id} className="space-y-1">
                                            <div className="flex justify-between items-center gap-2">
                                                <div className="flex-1">
                                                    <div className="flex justify-between items-center">
                                                        <span className="text-white font-bold text-base">{stat.name}</span>
                                                        <span className="text-blue-400 font-bold text-lg">
                                                            {stat.currentValue.toFixed(2)}
                                                        </span>
                                                    </div>
                                                </div>
                                                {profile && profile.availableStatPoints > 0 && (
                                                    <button
                                                        onClick={async () => {
                                                            const token = localStorage.getItem('token');
                                                            if (!token) return;
                                                            try {
                                                                const updatedProfile = await api.character.distributeStat(token, stat.id);
                                                                setProfile(updatedProfile);
                                                                const updatedStats = await api.character.getStats(token);
                                                                setStats(updatedStats);
                                                                showNotification(`+1 ${stat.name}!`, 'success');
                                                            } catch (error) {
                                                                console.error('Failed to distribute stat:', error);
                                                                showNotification('Failed to distribute stat point', 'error');
                                                            }
                                                        }}
                                                        className="px-2 py-1 bg-green-500/20 hover:bg-green-500/30 text-green-400 rounded transition-all text-sm font-bold"
                                                        title="Add 1 point"
                                                    >
                                                        +
                                                    </button>
                                                )}
                                            </div>
                                            <div className="h-2 bg-white/5 rounded-full overflow-hidden">
                                                <div
                                                    className="h-full bg-gradient-to-r from-blue-500 to-purple-500"
                                                    style={{ width: `${(stat.currentValue / stat.maxValue) * 100}%` }}
                                                ></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>

                            {/* Fatigue */}
                            <div className="mt-6">
                                <FatigueMeter
                                    fatigueLevel={fatigue.level}
                                    questsToday={fatigue.questsToday}
                                    recommendedMax={fatigue.recommendedMax}
                                />
                            </div>
                        </div>

                        {/* Habits Content */}
                        <div className="lg:col-span-2">
                            <div className="glass rounded-2xl p-6">
                                <h2 className="text-2xl font-bold mb-6">Habits</h2>
                                <p className="text-gray-400">Coming soon...</p>
                            </div>
                        </div>
                    </div>
                )}

                {/* Journal Tab */}
                {activeTab === 'journal' && (
                    <JournalTab
                        entries={journalEntries}
                        onAddEntry={async (content) => {
                            const token = localStorage.getItem('token');
                            if (!token) return;

                            try {
                                await api.journal.create(token, content);
                                const entries = await api.journal.getEntries(token);
                                setJournalEntries(entries);
                                showNotification('Journal entry saved!', 'success');
                            } catch (error) {
                                console.error('Failed to save journal entry:', error);
                                showNotification('Failed to save entry', 'error');
                            }
                        }}
                        onUpdateEntry={async (id, content) => {
                            const token = localStorage.getItem('token');
                            if (!token) return;

                            try {
                                await api.journal.update(token, id, content);
                                const entries = await api.journal.getEntries(token);
                                setJournalEntries(entries);
                                showNotification('Journal entry updated!', 'success');
                            } catch (error) {
                                console.error('Failed to update journal entry:', error);
                                showNotification('Failed to update entry', 'error');
                            }
                        }}
                        onDeleteEntry={async (id) => {
                            const token = localStorage.getItem('token');
                            if (!token) return;

                            try {
                                await api.journal.delete(token, id);
                                const entries = await api.journal.getEntries(token);
                                setJournalEntries(entries);
                                showNotification('Journal entry deleted!', 'success');
                            } catch (error) {
                                console.error('Failed to delete journal entry:', error);
                                showNotification('Failed to delete entry', 'error');
                            }
                        }}
                    />
                )}
            </div>

            {/* Create Quest Modal */}
            {showCreateModal && (
                <CreateQuestModal
                    isOpen={showCreateModal}
                    onClose={() => setShowCreateModal(false)}
                    onSubmit={handleCreateQuest}
                />
            )}

            {/* Notification */}
            {notification && (
                <Notification
                    message={notification.message}
                    type={notification.type}
                    onClose={() => setNotification(null)}
                />
            )}
        </div>

    );
}
