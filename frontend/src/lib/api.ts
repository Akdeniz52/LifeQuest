// API Client
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5240/api';

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  email: string;
  password: string;
  characterName: string;
}

interface AuthResponse {
  token: string;
  characterId: string;
  characterName: string;
  level: number;
}

export const api = {
  auth: {
    login: async (data: LoginRequest): Promise<AuthResponse> => {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });
      if (!res.ok) throw new Error('Login failed');
      return res.json();
    },

    register: async (data: RegisterRequest): Promise<AuthResponse> => {
      const res = await fetch(`${API_URL}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });
      if (!res.ok) throw new Error('Registration failed');
      return res.json();
    },
  },

  character: {
    getProfile: async (token: string) => {
      const res = await fetch(`${API_URL}/character`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch profile');
      return res.json();
    },

    getStats: async (token: string) => {
      const res = await fetch(`${API_URL}/character/stats`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch stats');
      return res.json();
    },
    distributeStat: async (token: string, statId: string, amount: number = 1) => {
      const res = await fetch(`${API_URL}/character/distribute-stat`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ statId, amount }),
      });
      if (!res.ok) throw new Error('Failed to distribute stat point');
      return res.json();
    },
  },

  quests: {
    getToday: async (token: string) => {
      const res = await fetch(`${API_URL}/quests/today`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch quests');
      return res.json();
    },

    complete: async (token: string, questId: string) => {
      const res = await fetch(`${API_URL}/quests/${questId}/complete`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to complete quest');
      return res.json();
    },

    fail: async (token: string, questId: string) => {
      const res = await fetch(`${API_URL}/quests/${questId}/fail`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fail quest');
      return res.json();
    },

    create: async (token: string, questData: any) => {
      const res = await fetch(`${API_URL}/quests/definitions`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(questData),
      });
      if (!res.ok) throw new Error('Failed to create quest');
      return res.json();
    },

    delete: async (token: string, questId: string) => {
      const res = await fetch(`${API_URL}/quests/definitions/${questId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to delete quest');
      return res.json();
    },
  },

  skills: {
    getAvailable: async (token: string) => {
      const res = await fetch(`${API_URL}/skills/available`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch skills');
      return res.json();
    },

    getUnlocked: async (token: string) => {
      const res = await fetch(`${API_URL}/skills/unlocked`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch unlocked skills');
      return res.json();
    },

    unlock: async (token: string, skillId: string) => {
      const res = await fetch(`${API_URL}/skills/${skillId}/unlock`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to unlock skill');
      return res.json();
    },

    use: async (token: string, skillId: string) => {
      const res = await fetch(`${API_URL}/skills/${skillId}/use`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to use skill');
      return res.json();
    },
  },

  analytics: {
    getSummary: async (token: string, days: number = 30) => {
      const res = await fetch(`${API_URL}/analytics/summary?days=${days}`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch summary');
      return res.json();
    },
  },

  journal: {
    getEntries: async (token: string) => {
      const res = await fetch(`${API_URL}/journal`, {
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to fetch journal entries');
      return res.json();
    },

    create: async (token: string, content: string) => {
      const res = await fetch(`${API_URL}/journal`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ content }),
      });
      if (!res.ok) throw new Error('Failed to create journal entry');
      return res.json();
    },

    update: async (token: string, id: string, content: string) => {
      const res = await fetch(`${API_URL}/journal/${id}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ content }),
      });
      if (!res.ok) throw new Error('Failed to update journal entry');
      return res.json();
    },

    delete: async (token: string, id: string) => {
      const res = await fetch(`${API_URL}/journal/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Failed to delete journal entry');
      return res.json();
    },
  },
};
