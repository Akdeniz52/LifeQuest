interface NotificationProps {
    message: string;
    type: 'success' | 'error' | 'info';
    onClose: () => void;
}

export default function Notification({ message, type, onClose }: NotificationProps) {
    const colors = {
        success: 'from-green-500 to-emerald-500',
        error: 'from-red-500 to-pink-500',
        info: 'from-blue-500 to-purple-500',
    };

    const icons = {
        success: '✓',
        error: '✕',
        info: 'ℹ',
    };

    return (
        <div className="fixed top-4 right-4 z-50 animate-slideUp">
            <div className={`glass rounded-lg p-4 flex items-center gap-3 bg-gradient-to-r ${colors[type]} glow`}>
                <div className="text-2xl">{icons[type]}</div>
                <div className="flex-1 text-white font-semibold">{message}</div>
                <button
                    onClick={onClose}
                    className="text-white/80 hover:text-white transition-colors"
                >
                    ✕
                </button>
            </div>
        </div>
    );
}
