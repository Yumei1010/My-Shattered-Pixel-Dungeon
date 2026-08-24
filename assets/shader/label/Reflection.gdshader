shader_type canvas_item;

// 镜像开始的位置（UV 的 V 坐标），0.5 表示正中
uniform float mirror_start : hint_range(0.0, 1.0) = 0.5;
// 镜像渐隐强度，1.0 表示底部完全透明
uniform float mirror_fade : hint_range(0.0, 1.0) = 0.6;
// 水波扭曲的幅度
uniform float wave_amplitude : hint_range(0.0, 0.05) = 0.015;
// 水波频率
uniform float wave_frequency : hint_range(1.0, 20.0) = 8.0;

void fragment() {
	vec2 uv = UV;
	vec4 col;

	if (uv.y < mirror_start) {
		// 上半部分：原样显示
		col = texture(TEXTURE, uv);
	} else {
		// 下半部分：倒影
		// 计算对称的 V 坐标（以 mirror_start 为对称轴）
		float mirrored_v = 2.0 * mirror_start - uv.y;
		vec2 mirrored_uv = vec2(uv.x, mirrored_v);

		// 水波扭曲：让倒影的水平方向随时间和位置发生偏移
		float wave = sin(uv.y * wave_frequency + TIME * 2.5) * wave_amplitude;
		mirrored_uv.x += wave;

		// 采样纹理（倒影内容）
		col = texture(TEXTURE, mirrored_uv);

		// 渐变透明度：从 mirror_start 向下逐渐变淡
		float fade = smoothstep(mirror_start, 1.0, uv.y) * mirror_fade;
		col.a *= 1.0 - fade;
	}

	COLOR = col;
}