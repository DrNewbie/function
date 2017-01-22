-- Example: local ids = idstring_table()
-- Returns: table of all idstrings, indexed by their idstring key
function idstring_table()
	local ids = {}
	if( DB:has( "idstring_lookup", "idstring_lookup" ) ) then
		local file = DB:open( "idstring_lookup", "idstring_lookup" )
		local data = file:read()
   
		for _,text in pairs( string.split( data, '%z' ) ) do
			ids[ text:id():key() ] = text
		end
   
		file:close()
	end 
	return ids
end

local _ids = idstring_table()
if _ids and type(_ids) == "table" and #_ids > 0 then	
	local filess = io.open("idstring_lookup_[all_result].txt", "w")
	if filess then
		filess:write(json.encode(_ids))
		filess:close()
	end
end
