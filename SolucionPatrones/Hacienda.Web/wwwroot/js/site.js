// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// JS global del sitio
// Animaciones y mejoras UI sin alterar lógica
(function () {
 document.addEventListener('DOMContentLoaded', function () {
 // Auto-cerrar alertas con tiempo configurable (15 segundos por defecto)
 const alerts = document.querySelectorAll('.alert');
 alerts.forEach(function (al) {
 const timeoutAttr = al.getAttribute('data-timeout');
 const timeout = timeoutAttr ? parseInt(timeoutAttr,10) : 15000; // 15 segundos
 if (!isNaN(timeout) && timeout >0) {
 setTimeout(() => {
 al.classList.add('fade');
 const btn = al.querySelector('.btn-close');
 if (btn) btn.click(); else al.remove();
 }, timeout);
 }
 });

 // Revelar tarjetas con efecto sutil
 const reveal = document.querySelectorAll('.aw-card, .aw-page-header, .aw-reveal');
 const onScroll = () => {
 const trigger = window.innerHeight *0.92;
 reveal.forEach(el => {
 const top = el.getBoundingClientRect().top;
 if (top < trigger) el.classList.add('aw-show');
 });
 };
 onScroll();
 window.addEventListener('scroll', onScroll, { passive: true });

 // Hover sutil en botones aw-btn
 document.querySelectorAll('.aw-btn').forEach(btn => {
 btn.addEventListener('mouseenter', () => btn.classList.add('shadow-sm'));
 btn.addEventListener('mouseleave', () => btn.classList.remove('shadow-sm'));
 });
 });
})();
