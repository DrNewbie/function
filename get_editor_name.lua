local _table = {}

for _, script in pairs(managers.mission:scripts()) do
	for _,element in pairs(script:elements()) do
		local _name = element:editor_name()
		local _id = "id_" .. element:id()
		_table[_id] = _name
	end
end

log("get_editor_name: " .. tostring(json.encode(_table)))
