/** WCAG 2.x contrast ratio between two opaque `rgb(r, g, b)` colours. */
export function contrastRatio(a: string, b: string): number {
  const [la, lb] = [a, b].map(luminance).sort((x, y) => y - x);
  return (la + 0.05) / (lb + 0.05);
}

function luminance(rgb: string): number {
  const [r, g, b] = (rgb.match(/\d+(\.\d+)?/g) ?? []).slice(0, 3).map((c) => {
    const v = Number(c) / 255;
    return v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
  });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}
