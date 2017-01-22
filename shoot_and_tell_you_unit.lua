local _f_NewRaycastWeaponBase_fire = NewRaycastWeaponBase.fire
function NewRaycastWeaponBase:fire(...)
	local _bool = true
	if _bool then
		--Copy from 'pierredjays'
		local camera = managers.player:player_unit():movement()._current_state._ext_camera
		local mvec_to = Vector3()
		local from_pos = camera:position()
		mvector3.set( mvec_to, camera:forward() )
		mvector3.multiply( mvec_to, 20000 )
		mvector3.add( mvec_to, from_pos )
		local col_ray = World:raycast( "ray", from_pos, mvec_to, "slot_mask", managers.slot:get_mask( "all" ) )
		if col_ray and col_ray.unit then
			log("col_ray.unit: " .. tostring(col_ray.unit))
			--col_ray.unit:set_slot(0)
		end
	end
	return _f_NewRaycastWeaponBase_fire(self, ...)
end
