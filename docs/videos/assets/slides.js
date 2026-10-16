// Shared slide runtime. One slide is visible at a time.
//   slides.html?slide=3  shows the third slide (the video builder uses this).
//   Open without a parameter and use the arrow keys or space to step through.
(function () {
  const body = document.body;
  const slides = Array.from(document.querySelectorAll('body > section'));
  const parts = (body.dataset.parts || '').split('|');

  const header = document.createElement('header');
  header.className = 'deck-header';
  header.innerHTML =
    '<span class="deck-title"></span><span class="deck-part"></span>' +
    '<div class="deck-progress"><i></i></div>';
  header.querySelector('.deck-title').textContent = body.dataset.videoTitle || '';
  body.prepend(header);

  function show(n) {
    const index = Math.min(Math.max(n, 1), slides.length) - 1;
    slides.forEach((s, i) => s.classList.toggle('active', i === index));
    const part = Number(slides[index].dataset.part || 0);
    header.querySelector('.deck-part').textContent = parts[part] || '';
    header.querySelector('.deck-progress i').style.width = ((index + 1) / slides.length) * 100 + '%';
    return index + 1;
  }

  const query = new URLSearchParams(location.search).get('slide') || location.hash.slice(1);
  let current = show(Number(query) || 1);

  document.addEventListener('keydown', (e) => {
    if (['ArrowRight', 'PageDown', ' '].includes(e.key)) current = show(current + 1);
    if (['ArrowLeft', 'PageUp'].includes(e.key)) current = show(current - 1);
  });
})();
