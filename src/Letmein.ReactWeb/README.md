# Letmein.ReactWeb

A modern, secure text encryption and sharing application built with React, Vite, and Tailwind CSS.

## Features

- Client-side encryption using SJCL (Stanford Javascript Crypto Library)
- Modern, responsive UI with Tailwind CSS
- Temporary encrypted message storage with expiry
- Countdown timer for message expiration
- Copy-to-clipboard functionality
- Clean, intuitive user experience

## Prerequisites

- Node.js (v16 or higher)
- Running instance of Letmein.API

## Setup

1. Install dependencies:
```bash
npm install
```

2. Configure the API URL:
   - Copy `.env.example` to `.env`
   - Update `VITE_API_URL` with your API endpoint

```bash
cp .env.example .env
```

3. Start the development server:
```bash
npm run dev
```

4. Build for production:
```bash
npm run build
```

## Usage

### Creating an Encrypted Message

1. Navigate to the home page
2. Enter your secret message in the text area
3. Create a password (minimum 5 characters)
4. Select an expiry time
5. Click "Encrypt & Share"
6. Copy the generated URL and share it

### Decrypting a Message

1. Open the shared URL
2. Enter the password
3. Click "Decrypt" to view the message
4. The countdown timer shows when the message will expire
5. Optionally delete the message before it expires

## Technology Stack

- **React** - UI framework
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework
- **React Router** - Client-side routing
- **SJCL** - Client-side encryption library

## API Integration

The app communicates with the Letmein.API backend for:
- Fetching available expiry times
- Storing encrypted messages
- Loading encrypted messages
- Deleting messages

All encryption/decryption happens client-side - the server never sees the plain text.

## Environment Variables

- `VITE_API_URL` - Base URL for the Letmein API (default: `http://localhost:5000`)

## Development

The project structure:

```
src/
├── components/     # Reusable UI components
│   ├── Countdown.jsx
│   └── Toast.jsx
├── pages/         # Page components
│   ├── Home.jsx
│   └── Decrypt.jsx
├── utils/         # Utility functions
│   └── api.js
├── App.jsx        # Main app with routing
└── main.jsx       # Entry point
```

## Security

- All encryption/decryption happens in the browser
- The server only stores encrypted ciphertext
- Passwords are never transmitted to the server
- Messages automatically expire based on user selection
