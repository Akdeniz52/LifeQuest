'use client';

import { useState } from 'react';

interface JournalEntry {
    id: string;
    date: string;
    content: string;
}

interface JournalTabProps {
    entries: JournalEntry[];
    onAddEntry: (content: string) => void;
    onUpdateEntry: (id: string, content: string) => void;
    onDeleteEntry: (id: string) => void;
}

export default function JournalTab({ entries, onAddEntry, onUpdateEntry, onDeleteEntry }: JournalTabProps) {
    const [newEntry, setNewEntry] = useState('');
    const [selectedEntry, setSelectedEntry] = useState<JournalEntry | null>(null);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editContent, setEditContent] = useState('');

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (newEntry.trim()) {
            onAddEntry(newEntry);
            setNewEntry('');
        }
    };

    const startEdit = (entry: JournalEntry) => {
        setEditingId(entry.id);
        setEditContent(entry.content);
    };

    const cancelEdit = () => {
        setEditingId(null);
        setEditContent('');
    };

    const saveEdit = (id: string) => {
        if (editContent.trim()) {
            onUpdateEntry(id, editContent);
            setEditingId(null);
            setEditContent('');
        }
    };

    const handleDelete = (id: string) => {
        if (confirm('Are you sure you want to delete this journal entry?')) {
            onDeleteEntry(id);
            if (selectedEntry?.id === id) {
                setSelectedEntry(null);
            }
        }
    };

    const today = new Date().toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });

    return (
        <div className="space-y-6">
            {/* Today's Entry */}
            <div className="glass rounded-2xl p-6">
                <h2 className="text-2xl font-bold mb-2 text-gradient">Today's Journal</h2>
                <p className="text-gray-400 text-sm mb-4">{today}</p>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <textarea
                        value={newEntry}
                        onChange={(e) => setNewEntry(e.target.value)}
                        className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all resize-none"
                        placeholder="What did you accomplish today? How do you feel?"
                        rows={6}
                    />
                    <button
                        type="submit"
                        className="px-6 py-3 bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 rounded-lg font-semibold transition-all"
                    >
                        Save Entry
                    </button>
                </form>
            </div>

            {/* Past Entries */}
            <div className="glass rounded-2xl p-6">
                <h2 className="text-2xl font-bold mb-6">Past Entries</h2>
                {entries.length === 0 ? (
                    <div className="text-center py-12 text-gray-400">
                        <p className="text-lg mb-2">No journal entries yet</p>
                        <p className="text-sm">Start writing to track your journey!</p>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {entries.map((entry) => (
                            <div
                                key={entry.id}
                                className="glass glass-hover rounded-xl p-4 border border-white/10"
                            >
                                <div className="flex items-center justify-between mb-2">
                                    <h3
                                        className="text-lg font-semibold cursor-pointer flex-1"
                                        onClick={() => setSelectedEntry(selectedEntry?.id === entry.id ? null : entry)}
                                    >
                                        {new Date(entry.date).toLocaleDateString('en-US', {
                                            month: 'long',
                                            day: 'numeric',
                                            year: 'numeric'
                                        })}
                                    </h3>
                                    <div className="flex items-center gap-2">
                                        {editingId !== entry.id && (
                                            <>
                                                <button
                                                    onClick={() => startEdit(entry)}
                                                    className="px-3 py-1 bg-blue-500/20 hover:bg-blue-500/30 text-blue-400 rounded text-sm transition-all"
                                                    title="Edit"
                                                >
                                                    ✏️ Edit
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(entry.id)}
                                                    className="px-3 py-1 bg-red-500/20 hover:bg-red-500/30 text-red-400 rounded text-sm transition-all"
                                                    title="Delete"
                                                >
                                                    🗑️ Delete
                                                </button>
                                            </>
                                        )}
                                        <span
                                            className="text-sm text-gray-400 cursor-pointer"
                                            onClick={() => setSelectedEntry(selectedEntry?.id === entry.id ? null : entry)}
                                        >
                                            {selectedEntry?.id === entry.id ? '▼' : '▶'}
                                        </span>
                                    </div>
                                </div>

                                {selectedEntry?.id === entry.id && (
                                    <div className="mt-4 pt-4 border-t border-white/10">
                                        {editingId === entry.id ? (
                                            <div className="space-y-3">
                                                <textarea
                                                    value={editContent}
                                                    onChange={(e) => setEditContent(e.target.value)}
                                                    className="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all resize-none"
                                                    rows={6}
                                                />
                                                <div className="flex gap-2">
                                                    <button
                                                        onClick={() => saveEdit(entry.id)}
                                                        className="px-4 py-2 bg-green-500/20 hover:bg-green-500/30 text-green-400 rounded transition-all"
                                                    >
                                                        ✓ Save
                                                    </button>
                                                    <button
                                                        onClick={cancelEdit}
                                                        className="px-4 py-2 bg-gray-500/20 hover:bg-gray-500/30 text-gray-400 rounded transition-all"
                                                    >
                                                        ✕ Cancel
                                                    </button>
                                                </div>
                                            </div>
                                        ) : (
                                            <p className="text-gray-300 whitespace-pre-wrap">{entry.content}</p>
                                        )}
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
