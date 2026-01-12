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
    
    // Obtener la URL del API desde variables de entorno (.env)
    const apiUrl = process.env.API_URL;
    
    if (!apiUrl) {
      console.error('API_URL no está configurada en las variables de entorno');
      return NextResponse.json(
        { error: 'Configuración del servidor incorrecta' },
        { status: 500 }
      );
    }
    
    console.log(`Proxying login request to: ${apiUrl}/api/auth/login`);
    
    const response = await fetch(`${apiUrl}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(body),
    });

    const responseText = await response.text();
    console.log(`Response status: ${response.status}, body: ${responseText.substring(0, 100)}`);

    if (!response.ok) {
      let errorData;
      try {
        errorData = responseText ? JSON.parse(responseText) : { error: 'Error al iniciar sesión' };
      } catch {
        errorData = { error: responseText || 'Error al iniciar sesión' };
      }
      return NextResponse.json(
        errorData,
        { status: response.status }
      );
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
    console.error('Error en proxy de login:', error);
    return NextResponse.json(
      { error: error instanceof Error ? error.message : 'Error al procesar la solicitud' },
      { status: 500 }
    );
  }
}
