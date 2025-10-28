import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import sjcl from 'sjcl';
import { api } from '../utils/api';
import Toast from '../components/Toast';

export default function Home() {
  const [text, setText] = useState('');
  const [password, setPassword] = useState('');
  const [expiryTime, setExpiryTime] = useState('');
  const [expiryTimes, setExpiryTimes] = useState({});
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [toast, setToast] = useState(null);

  useEffect(() => {
    loadExpiryTimes();
  }, []);

  const loadExpiryTimes = async () => {
    try {
      const times = await api.getExpiryTimes();
      setExpiryTimes(times);
      // Set default to first option
      const firstKey = Object.keys(times)[0];
      if (firstKey) {
        setExpiryTime(firstKey);
      }
    } catch (error) {
      showToast('Failed to load expiry times', 'error');
    }
  };

  const showToast = (message, type = 'success') => {
    setToast({ message, type });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!text.trim()) {
      showToast('Please enter some text', 'error');
      return;
    }

    if (password.length < 5) {
      showToast('Password must be at least 5 characters', 'error');
      return;
    }

    setLoading(true);

    try {
      // Encrypt the text client-side
      const cipherJson = sjcl.encrypt(password, text);

      // Store the encrypted data
      const response = await api.storeNote(cipherJson, parseInt(expiryTime));

      setResult({
        friendlyId: response.friendlyId,
        expiresIn: response.expiresIn,
      });

      // Clear the form
      setText('');
      setPassword('');

      showToast('Text encrypted successfully!', 'success');
    } catch (error) {
      showToast(error.message || 'Failed to encrypt text', 'error');
    } finally {
      setLoading(false);
    }
  };

  const copyToClipboard = async () => {
    const url = `${window.location.origin}/${result.friendlyId}`;
    try {
      await navigator.clipboard.writeText(url);
      showToast('Copied to clipboard!', 'success');
    } catch (error) {
      showToast('Failed to copy to clipboard', 'error');
    }
  };

  const createAnother = () => {
    setResult(null);
  };

  if (result) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
        {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
        <div className="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-2xl">
          <div className="flex justify-center mb-8">
            <Link to="/">
              <img src="/logo.png" alt="Letmein Logo" className="w-24 h-24 cursor-pointer hover:opacity-80 transition-opacity" />
            </Link>
          </div>

          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-800 mb-2">Success!</h1>
            <p className="text-gray-600">Your text has been encrypted</p>
            <p className="text-sm text-gray-500 mt-2">It will expire in {result.expiresIn}</p>
          </div>

          <div className="bg-gray-50 rounded-lg p-6 mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Share this URL:
            </label>
            <div className="flex items-center space-x-2">
              <input
                type="text"
                value={`${window.location.origin}/${result.friendlyId}`}
                readOnly
                className="flex-1 px-4 py-3 border border-gray-300 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <button
                onClick={copyToClipboard}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium flex items-center space-x-2"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3" />
                </svg>
                <span>Copy</span>
              </button>
            </div>
          </div>

          <button
            onClick={createAnother}
            className="w-full px-6 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors font-medium"
          >
            Create Another
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}

      <div className="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-3xl">
        <div className="flex items-center justify-center gap-4 mb-8">
          <Link to="/">
            <img src="/logo.png" alt="Letmein Logo" className="w-24 h-24 cursor-pointer hover:opacity-80 transition-opacity" />
          </Link>
          <div>
            <h1 className="text-4xl font-bold text-gray-800 mb-1">Letmein</h1>
            <p className="text-gray-600">Secure, encrypted text sharing</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <textarea
              id="text"
              value={text}
              onChange={(e) => setText(e.target.value)}
              rows={12}
              placeholder="Enter the text you want to encrypt..."
              className="w-full px-4 py-3 border-0 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none text-white placeholder-gray-400"
              style={{ backgroundColor: '#1F1F1F' }}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                Password (min 5 characters)
              </label>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Create a password"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label htmlFor="expiry" className="block text-sm font-medium text-gray-700 mb-2">
                Expires In
              </label>
              <select
                id="expiry"
                value={expiryTime}
                onChange={(e) => setExpiryTime(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
              >
                {Object.entries(expiryTimes).map(([key, value]) => (
                  <option key={key} value={key}>
                    {value}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full px-6 py-4 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-semibold text-lg disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Encrypting...' : 'Encrypt'}
          </button>
        </form>
      </div>
    </div>
  );
}
