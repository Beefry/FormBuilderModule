angular.module('formbuilder')
.service('templateAPI',['$resource','templateAPIPath',function($resource,templateAPIPath){
	this.save = function(form,callback) {
		Form = $resource(templateAPIPath);
		// console.log(form);
		Form.save(form,function(data){
			callback(data);
		});
	}
	this.get = function(id, callback) {
		Form = $resource(templateAPIPath + ":ID");
		Form.get({ID:id},function(data){
			callback(data);
		});
	};
}]);