import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import sjcl from 'sjcl';
import { api } from '../utils/api';
import Toast from '../components/Toast';
import Countdown from '../components/Countdown';

export default function Decrypt() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [password, setPassword] = useState('');
  const [cipherJson, setCipherJson] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [decryptedText, setDecryptedText] = useState('');
  const [isDecrypted, setIsDecrypted] = useState(false);
  const [toast, setToast] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadNote();
  }, [id]);

  const loadNote = async () => {
    try {
      const data = await api.loadNote(id);
      setCipherJson(data.cipherJson);
      setExpiryDate(data.expiryDate);
      setLoading(false);
    } catch (error) {
      setError(error.message);
      setLoading(false);
    }
  };

  const showToast = (message, type = 'success') => {
    setToast({ message, type });
  };

  const handleDecrypt = (e) => {
    e.preventDefault();

    if (!password) {
      showToast('Please enter a password', 'error');
      return;
    }

    try {
      const decrypted = sjcl.decrypt(password, cipherJson);
      setDecryptedText(decrypted);
      setIsDecrypted(true);
      showToast('Successfully decrypted!', 'success');
    } catch (error) {
      showToast('Unable to decrypt. Wrong password?', 'error');
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this note?')) {
      return;
    }

    try {
      await api.deleteNote(id);
      showToast('Note deleted successfully', 'success');
      setTimeout(() => {
        navigate('/');
      }, 1500);
    } catch (error) {
      showToast('Failed to delete note', 'error');
    }
  };

  const handleExpire = () => {
    showToast('This note has expired', 'error');
    setTimeout(() => {
      navigate('/');
    }, 2000);
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter') {
      handleDecrypt(e);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600 text-lg">Loading...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-2xl text-center">
          <div className="flex justify-center mb-6">
            <Link to="/">
              <img src="/logo.png" alt="Letmein Logo" className="w-24 h-24 cursor-pointer hover:opacity-80 transition-opacity" />
            </Link>
          </div>
          <div className="text-red-500 mb-6">
            <svg className="w-16 h-16 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <h2 className="text-2xl font-bold mb-2">Error</h2>
            <p className="text-gray-600">{error}</p>
          </div>
          <button
            onClick={() => navigate('/')}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            Go Home
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

        {!isDecrypted ? (
          <div>
            <div className="text-center mb-8">
              <h1 className="text-3xl font-bold text-gray-800 mb-2">Enter Password</h1>
              <p className="text-gray-600">Enter the password to decrypt the message</p>
            </div>

            <form onSubmit={handleDecrypt} className="space-y-6">
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                  Password
                </label>
                <input
                  type="password"
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onKeyPress={handleKeyPress}
                  placeholder="Enter the password"
                  autoFocus
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <button
                type="submit"
                className="w-full px-6 py-4 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-semibold text-lg"
              >
                Decrypt
              </button>
            </form>
          </div>
        ) : (
          <div>
            <div className="text-center mb-6">
              <h1 className="text-3xl font-bold text-gray-800 mb-2">Decrypted note</h1>
              <div className="flex justify-center items-center gap-4 mt-4">
                {expiryDate && (
                  <Countdown targetDate={expiryDate} onExpire={handleExpire} />
                )}
                <button
                  onClick={handleDelete}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium text-sm"
                >
                  Delete
                </button>
              </div>
            </div>

            <div className="bg-gray-50 rounded-lg p-6">
              <textarea
                value={decryptedText}
                readOnly
                rows={15}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none resize-none text-white"
                style={{ backgroundColor: '#1F1F1F' }}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
