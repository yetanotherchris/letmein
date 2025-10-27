// This file must be imported FIRST to set up globals before anything else
import $ from 'jquery';

// IMMEDIATELY and EXPLICITLY expose jQuery to window - this must happen synchronously
// Using explicit property assignment to avoid any bundler optimization issues
if (typeof window !== 'undefined') {
  window['jQuery'] = $;
  window['$'] = $;

  // Double-check it's set
  if (typeof window.jQuery === 'undefined') {
    console.error('FAILED to expose jQuery globally!');
  } else {
    console.log('✓ jQuery exposed globally successfully');
  }
}

export { $ };
