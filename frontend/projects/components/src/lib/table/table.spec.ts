import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Matrix } from './matrix';
import { Table } from './table';
import { TableWrap } from './table-wrap';

@Component({
  imports: [TableWrap, Table, Matrix],
  template: `
    <div w-table-wrap>
      <table w-table cards aria-label="People">
        <thead>
          <tr>
            <th>Name</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td data-label="Name">Ana</td>
          </tr>
        </tbody>
      </table>
    </div>
    <div w-table-wrap>
      <table w-table>
        <tbody>
          <tr>
            <td>Plain</td>
          </tr>
        </tbody>
      </table>
    </div>
    <table w-matrix aria-label="Progress board">
      <tbody>
        <tr>
          <td>Maya</td>
        </tr>
      </tbody>
    </table>
  `,
})
class Host {}

describe('Table', () => {
  async function render() {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('wraps a native table and keeps its markup', async () => {
    const root = await render();
    const wrap = root.querySelector('div')!;
    expect(wrap.classList.contains('w-table-wrap')).toBe(true);
    const table = wrap.querySelector('table')!;
    expect(table.classList.contains('w-table')).toBe(true);
    expect(table.getAttribute('aria-label')).toBe('People');
    expect(table.querySelector('td')?.getAttribute('data-label')).toBe('Name');
  });

  it('turns rows into cards on small screens only when asked', async () => {
    const [cards, plain] = Array.from((await render()).querySelectorAll('table.w-table'));
    expect(cards.classList.contains('w-table--cards')).toBe(true);
    expect(plain.classList.contains('w-table--cards')).toBe(false);
  });

  it('draws a progress matrix', async () => {
    const matrix = (await render()).querySelector('table[w-matrix]')!;
    expect(matrix.classList.contains('w-matrix')).toBe(true);
  });
});
