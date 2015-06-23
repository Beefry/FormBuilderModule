angular.module('formbuilder').constant('formAPIPath','/API/Form/');

angular.module('formbuilder').service('formAPI',['$resource','formAPIPath',function($resource,formAPIPath){
	this.save = function(form,callback) {
		Form = $resource(formAPIPath);
		console.log(form);
		Form.save(form,function(data){
			callback(data);
		});
	}
}]);

angular.module('formbuilder').value('preLoadedFBObject',{thing:''});