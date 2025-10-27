// CRITICAL: Import globals setup FIRST - this must expose jQuery immediately
import { $ } from './globals.js';

// Now import Bootstrap (which depends on jQuery being globally available)
import 'bootstrap';

// Import other dependencies
import bootbox from 'bootbox';
import ClipboardJS from 'clipboard';
import 'jquery-countdown';
import 'jquery-toast-plugin';
import sjcl from 'sjcl';

// Import CSS - Bootstrap from npm, others from local files
import 'bootstrap/dist/css/bootstrap.min.css';
import 'jquery-toast-plugin/dist/jquery.toast.min.css';
import '../css/main.css';

// Expose remaining libraries globally for inline scripts
window.bootbox = bootbox;
window.Clipboard = ClipboardJS;
window.sjcl = sjcl;

// Import the letmein classes - these define IndexView, StoreView, LoadView
import './letmein.js';

// Verify all globals are set
console.log('Globals check:', {
  jQuery: typeof window.jQuery !== 'undefined',
  $: typeof window.$ !== 'undefined',
  bootbox: typeof window.bootbox !== 'undefined',
  Clipboard: typeof window.Clipboard !== 'undefined',
  sjcl: typeof window.sjcl !== 'undefined',
  IndexView: typeof window.IndexView !== 'undefined',
  StoreView: typeof window.StoreView !== 'undefined',
  LoadView: typeof window.LoadView !== 'undefined'
});

// Log version info in development
if (import.meta.env.DEV) {
  console.log('Letmein - Development Mode');
}
