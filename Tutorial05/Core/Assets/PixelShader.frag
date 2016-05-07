#ifdef GL_ES
    precision highp float;
#endif
varying vec3 viewpos;
varying vec3 normal;
uniform vec3 albedo;
uniform float shininess;
uniform float specfactor;
uniform float shineGain;
uniform float specIntensity;
uniform vec3 specColor;
void main()
{
    vec3 nnormal = normalize(normal);

    // Diffuse
    vec3 lightdir = vec3(0, 0, -1);
    float intensityDiff = dot(nnormal, lightdir);

    // Specular
    float intensitySpec = 0.0;
    if (intensityDiff > 0.0)
    {
        vec3 viewdir = -viewpos;
		// if you dont normalize the interopolated normal vector, the viewdir and light dir just get summed up thus the whole model gets a evenlz distributed shine 
        vec3 h = normalize(viewdir+lightdir);
        intensitySpec = pow(max(0.0, dot(h, nnormal)), shininess);
    }

    gl_FragColor = vec4(intensityDiff * albedo + vec3(intensitySpec) * shineGain * specIntensity * vec3(specColor), 1);
}