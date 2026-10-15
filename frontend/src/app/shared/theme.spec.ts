import { DeliverySacPreset } from '../../theme/deliverysac-preset';
import { EstadoTagComponent } from './estado-tag/estado-tag.component';

describe('Tema y etiquetas de estado', () => {
  it('usa la paleta institucional #1976d2 como primario', () => {
    const preset = DeliverySacPreset as unknown as {
      semantic: {
        primary: Record<number, string>;
        colorScheme: { light: { primary: { color: string } } };
      };
    };

    expect(preset.semantic.primary[500]).toBe('#1976d2');
    expect(preset.semantic.colorScheme.light.primary.color).toBe('#1976d2');
  });

  it('cubre todos los estados del pedido con etiqueta y severidad', () => {
    const estados = ['Pendiente', 'Asignado', 'EnRuta', 'Entregado', 'NoEntregado', 'Reprogramado'];
    const componente = new EstadoTagComponent();

    for (const estado of estados) {
      componente.estado = estado;
      expect(componente.etiqueta).withContext(estado).toBeTruthy();
      expect(componente.severity).withContext(estado).toBeTruthy();
    }
  });
});
