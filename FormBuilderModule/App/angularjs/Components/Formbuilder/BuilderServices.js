angular.module('formbuilder')
.service('formAPI',['$resource','formAPIPath',function($resource,formAPIPath){
	this.save = function(form,callback) {
		Form = $resource(formAPIPath);
		// console.log(form);
		Form.save(form,function(data){
			callback(data);
		});
	}
	this.get = function(id, callback) {
		Form = $resource(formAPIPath + ":ID");
		Form.get({ID:id},function(data){
			callback(data);
		});
	};
}]);