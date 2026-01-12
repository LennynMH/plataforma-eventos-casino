'use client';

import { useState } from 'react';

interface Zone {
  id: string;
  name: string;
  price: number;
  capacity: number;
}

interface EventFormData {
  name: string;
  date: string;
  location: string;
  zones: Zone[];
}

interface EventRegistrationFormProps {
  token: string;
}

export default function EventRegistrationForm({ token }: EventRegistrationFormProps) {
  const [formData, setFormData] = useState<EventFormData>({
    name: '',
    date: '',
    location: '',
    zones: [{ id: '1', name: '', price: 0, capacity: 0 }]
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleInputChange = (field: keyof EventFormData, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setError(null);
  };

  const handleZoneChange = (zoneId: string, field: keyof Zone, value: string | number) => {
    setFormData(prev => ({
      ...prev,
      zones: prev.zones.map(zone =>
        zone.id === zoneId ? { ...zone, [field]: value } : zone
      )
    }));
    setError(null);
  };

  const addZone = () => {
    setFormData(prev => ({
      ...prev,
      zones: [...prev.zones, { id: Date.now().toString(), name: '', price: 0, capacity: 0 }]
    }));
  };

  const removeZone = (zoneId: string) => {
    if (formData.zones.length > 1) {
      setFormData(prev => ({
        ...prev,
        zones: prev.zones.filter(zone => zone.id !== zoneId)
      }));
    }
  };

  const validateForm = (): boolean => {
    // Validar campos obligatorios
    if (!formData.name.trim()) {
      setError('El nombre del evento es obligatorio');
      return false;
    }

    if (!formData.date) {
      setError('La fecha del evento es obligatoria');
      return false;
    }

    if (!formData.location.trim()) {
      setError('El lugar del evento es obligatorio');
      return false;
    }

    // Validar zonas
    for (const zone of formData.zones) {
      if (!zone.name.trim()) {
        setError('El nombre de la zona es obligatorio');
        return false;
      }

      if (zone.price < 0) {
        setError('El precio debe ser mayor o igual a 0');
        return false;
      }

      if (zone.capacity <= 0) {
        setError('La capacidad debe ser mayor a 0');
        return false;
      }
    }

    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:8070/api/events', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          name: formData.name,
          date: formData.date,
          location: formData.location,
          zones: formData.zones.map(zone => ({
            name: zone.name,
            price: zone.price,
            capacity: zone.capacity
          }))
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Error al crear el evento' }));
        throw new Error(errorData.message || `Error ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      setSuccess(true);
      
      // Reset form
      setFormData({
        name: '',
        date: '',
        location: '',
        zones: [{ id: '1', name: '', price: 0, capacity: 0 }]
      });

      console.log('Evento creado:', data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear el evento');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <h2 className="text-2xl font-semibold text-gray-800 mb-6">
        Registrar Evento
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Nombre del evento */}
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-2">
            Nombre del evento *
          </label>
          <input
            type="text"
            id="name"
            value={formData.name}
            onChange={(e) => handleInputChange('name', e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>

        {/* Fecha */}
        <div>
          <label htmlFor="date" className="block text-sm font-medium text-gray-700 mb-2">
            Fecha *
          </label>
          <input
            type="datetime-local"
            id="date"
            value={formData.date}
            onChange={(e) => handleInputChange('date', e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>

        {/* Lugar */}
        <div>
          <label htmlFor="location" className="block text-sm font-medium text-gray-700 mb-2">
            Lugar *
          </label>
          <input
            type="text"
            id="location"
            value={formData.location}
            onChange={(e) => handleInputChange('location', e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            required
          />
        </div>

        {/* Zonas */}
        <div>
          <div className="flex justify-between items-center mb-4">
            <label className="block text-sm font-medium text-gray-700">
              Zonas *
            </label>
            <button
              type="button"
              onClick={addZone}
              className="px-4 py-2 bg-green-500 text-white rounded-md hover:bg-green-600 transition-colors"
            >
              + Agregar Zona
            </button>
          </div>

          <div className="space-y-4">
            {formData.zones.map((zone, index) => (
              <div key={zone.id} className="border border-gray-200 rounded-md p-4 space-y-4">
                <div className="flex justify-between items-center mb-2">
                  <h3 className="text-sm font-medium text-gray-700">Zona {index + 1}</h3>
                  {formData.zones.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removeZone(zone.id)}
                      className="text-red-500 hover:text-red-700 text-sm"
                    >
                      Eliminar
                    </button>
                  )}
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">
                      Nombre *
                    </label>
                    <input
                      type="text"
                      value={zone.name}
                      onChange={(e) => handleZoneChange(zone.id, 'name', e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      required
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">
                      Precio *
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      value={zone.price}
                      onChange={(e) => handleZoneChange(zone.id, 'price', parseFloat(e.target.value) || 0)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      required
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1">
                      Capacidad *
                    </label>
                    <input
                      type="number"
                      min="1"
                      value={zone.capacity}
                      onChange={(e) => handleZoneChange(zone.id, 'capacity', parseInt(e.target.value) || 0)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      required
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Mensajes de error y éxito */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
            {error}
          </div>
        )}

        {success && (
          <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-md">
            ¡Evento creado exitosamente!
          </div>
        )}

        {/* Botón Guardar */}
        <div className="flex justify-end">
          <button
            type="submit"
            disabled={loading}
            className={`px-6 py-3 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 transition-colors ${
              loading ? 'opacity-50 cursor-not-allowed' : ''
            }`}
          >
            {loading ? 'Guardando...' : 'Guardar'}
          </button>
        </div>
      </form>
    </div>
  );
}
