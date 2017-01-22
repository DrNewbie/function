if not _bulletCollision then _bulletCollision = InstantBulletBase.on_collision end
function InstantBulletBase:on_collision( col_ray, weapon_unit, user_unit, damage, blank )
	if user_unit == managers.player:player_unit() then
		_lastBullet = col_ray.hit_position
		log("_lastBullet: " .. tostring(_lastBullet))
	end
	return _bulletCollision(self, col_ray, weapon_unit, user_unit, damage, blank)
end
