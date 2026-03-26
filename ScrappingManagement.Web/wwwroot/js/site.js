// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('themeToggle');
    if (!toggle) return;
    toggle.checked = (window.currentTheme === 'dark');
    toggle.addEventListener('click', function () {
        const next = this.checked ? 'dark' : 'light';

        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
    });
});
