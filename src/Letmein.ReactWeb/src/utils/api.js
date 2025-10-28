const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const api = {
  async getExpiryTimes() {
    const response = await fetch(`${API_BASE_URL}/api/pastes/expiry-times`);
    if (!response.ok) {
      throw new Error('Failed to fetch expiry times');
    }
    return await response.json();
  },

  async storePaste(cipherJson, expiryTime) {
    const response = await fetch(`${API_BASE_URL}/api/pastes`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        cipherJson,
        expiryTime,
      }),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to store note');
    }

    return await response.json();
  },

  async loadPaste(friendlyId) {
    const response = await fetch(`${API_BASE_URL}/api/pastes/${friendlyId}`);

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to load note');
    }

    return await response.json();
  },

  async deletePaste(friendlyId) {
    const response = await fetch(`${API_BASE_URL}/api/pastes/${friendlyId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to delete note');
    }

    return await response.json();
  },
};
