// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Gestion des formulaires avec validation
function setupFormValidation(formId, options = {}) {
    const form = document.getElementById(formId);
    if (!form) return;

    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        if (!this.checkValidity()) {
            e.stopPropagation();
            this.classList.add('was-validated');
            return;
        }

        const formData = new FormData(this);
        const submitButton = this.querySelector('button[type="submit"]');
        const originalButtonText = submitButton.innerHTML;

        try {
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Chargement...';

            const response = await fetch(this.action, {
                method: 'POST',
                body: options.useFormData ? formData : new URLSearchParams(formData),
                headers: options.useFormData ? {} : {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            });

            const data = await response.json();

            if (data.success) {
                if (options.onSuccess) {
                    options.onSuccess(data);
                } else {
                    Swal.fire({
                        icon: 'success',
                        title: 'Succès !',
                        text: data.message || 'Opération réussie',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }

                if (options.resetForm !== false) {
                    this.reset();
                    this.classList.remove('was-validated');
                }
            } else {
                if (options.onError) {
                    options.onError(data);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erreur',
                        text: data.message || 'Une erreur est survenue'
                    });
                }
            }
        } catch (error) {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Erreur',
                text: 'Une erreur est survenue lors de l\'opération'
            });
        } finally {
            submitButton.disabled = false;
            submitButton.innerHTML = originalButtonText;
        }
    });
}

// Initialisation des masques téléphoniques
function initPhoneMasks() {
    const phoneMaskElements = document.querySelectorAll('.phone-mask');
    phoneMaskElements.forEach(function(element) {
        IMask(element, {
            mask: '+{237} 000-000-000'
        });
    });
}

// Initialisation des formulaires dynamiques
document.addEventListener('DOMContentLoaded', function() {
    // Initialiser les masques téléphoniques
    initPhoneMasks();

    // Initialiser la validation du formulaire de correspondant
    setupFormValidation('correspondantForm', {
        onSuccess: function(data) {
            // Ajouter le nouveau correspondant à la liste déroulante
            const select = document.getElementById('CorrespondantId');
            if (select) {
                const option = new Option(data.nom, data.id, false, true);
                select.appendChild(option);
            }

            // Fermer le modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('correspondantModal'));
            if (modal) {
                modal.hide();
            }

            // Message de succès
            Swal.fire({
                icon: 'success',
                title: 'Succès !',
                text: data.message || 'Expéditeur enregistré avec succès',
                timer: 2000,
                showConfirmButton: false
            });
        }
    });
});
