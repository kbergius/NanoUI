cbuffer UniformBlock : register(b0, space1)
{
    float4x4 MatrixTransform : packoffset(c0);
};

struct Input
{
    float2 Position : POSITION;
    float2 UV : TEXCOORD0;
};

struct Output
{
    float4 Position : SV_Position; // vertex position
    float2 Ftcoord : TEXCOORD0;    // float 2 tex coord
    float2 Fpos : TEXCOORD1;       // float 2 position 
};

Output main(Input input)
{
    Output output;
    output.Position = mul(MatrixTransform, float4(input.Position, 0, 1.0f));
    output.Ftcoord = input.UV;
    output.Fpos = input.Position;

    return output;
}
