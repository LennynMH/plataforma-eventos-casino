import { NextRequest, NextResponse } from 'next/server';

export async function POST(request: NextRequest) {
  try {
    // Leer el body de forma segura
    let body;
    try {
      const text = await request.text();
      body = text ? JSON.parse(text) : {};
    } catch (parseError) {
      console.error('Error al parsear body:', parseError);
      return NextResponse.json(
        { error: 'Body inválido' },
        { status: 400 }
      );
    }

    const authHeader = request.headers.get('authorization');
    
    // Obtener la URL del API desde variables de entorno (.env)
    const apiUrl = process.env.API_URL;
    
    if (!apiUrl) {
      console.error('API_URL no está configurada en las variables de entorno');
      return NextResponse.json(
        { error: 'Configuración del servidor incorrecta' },
        { status: 500 }
      );
    }
    
    // Verificar que el token esté presente
    if (!authHeader) {
      console.error('Authorization header no encontrado en la request');
      return NextResponse.json(
        { error: 'Token de autenticación requerido' },
        { status: 401 }
      );
    }
    
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      'Authorization': authHeader, // Siempre incluir el header de autorización
    };
    
    console.log(`Proxying events request to: ${apiUrl}/api/events`);
    console.log(`Authorization header present: ${authHeader ? 'Yes' : 'No'}`);
    console.log(`Authorization header (first 50 chars): ${authHeader?.substring(0, 50)}...`);
    
    const response = await fetch(`${apiUrl}/api/events`, {
      method: 'POST',
      headers,
      body: JSON.stringify(body),
    });

    const responseText = await response.text();
    console.log(`Response status: ${response.status}, body: ${responseText.substring(0, 100)}`);

    if (!response.ok) {
      let errorData;
      try {
        errorData = responseText ? JSON.parse(responseText) : { message: 'Error al crear el evento' };
      } catch {
        errorData = { message: responseText || `Error ${response.status}: ${response.statusText}` };
      }
      return NextResponse.json(errorData, { status: response.status });
    }

    let data;
    try {
      data = responseText ? JSON.parse(responseText) : {};
    } catch (parseError) {
      console.error('Error al parsear respuesta:', parseError);
      return NextResponse.json(
        { error: 'Respuesta inválida del servidor' },
        { status: 500 }
      );
    }

    return NextResponse.json(data);
  } catch (error) {
    console.error('Error en proxy de events:', error);
    return NextResponse.json(
      { error: error instanceof Error ? error.message : 'Error al procesar la solicitud' },
      { status: 500 }
    );
  }
}
