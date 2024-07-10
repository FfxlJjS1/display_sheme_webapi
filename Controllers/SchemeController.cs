﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Xml;
using webapi.Kafka;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemeController : ControllerBase
    {
        private DbCollectionContext db;
        private bool is_injection_out_classification = true;
        private Dictionary<string, Dictionary<string, DownClassificationLevel[]>>? _injectionInOutClassification = null;
        private KafkaClient kafka_client = new KafkaClient("localhost::9092", KafkaClient.KafkaClientType.Producer);

        private Dictionary<string, Dictionary<string, DownClassificationLevel[]>> InjectionInOutClassification
        {
            get
            {
                if (_injectionInOutClassification == null)
                    DefineInjectionInOutClassification();

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
                return _injectionInOutClassification;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            }
        }

        public SchemeController(DbCollectionContext db)
        {
            this.db = db;
        }

        private void DefineInjectionInOutClassification()
        {
            var tipNpos = db.TipNpos.ToList();

            _injectionInOutClassification = new Dictionary<string, Dictionary<string, DownClassificationLevel[]>>();

            if (is_injection_out_classification)
            {
                var injectionOutGroupObjects = new Dictionary<string, DownClassificationLevel[]>(){
                { "Скважина", new DownClassificationLevel[] {
                    new DownClassificationLevel(1,
                        tipNpos.First(x => x.TipNpoId == 1).Name,
                        tipNpos.First(x => x.TipNpoId == 1).Name2)
                    }
                },
                { "Замерные пункты", new DownClassificationLevel[] {
                    new DownClassificationLevel(7,
                        tipNpos.First(x => x.TipNpoId == 7).Name,
                        tipNpos.First(x => x.TipNpoId == 7).Name2),
                    new DownClassificationLevel(12,
                        tipNpos.First(x => x.TipNpoId == 12).Name,
                        tipNpos.First(x => x.TipNpoId == 12).Name2)
                    }
                },
                { "Сборные пункты", new DownClassificationLevel[] {
                    new DownClassificationLevel(5,
                        tipNpos.First(x => x.TipNpoId == 5).Name,
                        tipNpos.First(x => x.TipNpoId == 5).Name2),
                    new DownClassificationLevel(6,
                        tipNpos.First(x => x.TipNpoId == 6).Name,
                        tipNpos.First(x => x.TipNpoId == 6).Name2)
                    }
                },
                { "Товарный парк", new DownClassificationLevel[] {
                    new DownClassificationLevel(10,
                        tipNpos.First(x => x.TipNpoId == 10).Name,
                        tipNpos.First(x => x.TipNpoId == 10).Name2)
                    }
                }
                };

                _injectionInOutClassification.Add("Объекты схемы сбора", injectionOutGroupObjects);
            }
            else
            {
                var injectionOutGroupObjects = new Dictionary<string, DownClassificationLevel[]>(){
                { "Скважины", new DownClassificationLevel[] {
                    new DownClassificationLevel(1,
                        tipNpos.First(x => x.TipNpoId == 1).Name,
                        tipNpos.First(x => x.TipNpoId == 1).Name2)
                    }
                },
                { "ВРП", new DownClassificationLevel[] {
                    new DownClassificationLevel(2,
                        tipNpos.First(x => x.TipNpoId == 2).Name,
                        tipNpos.First(x => x.TipNpoId == 2).Name2)
                    }
                },
                
                    { "КНС", new DownClassificationLevel[] {
                    new DownClassificationLevel(3,
                        tipNpos.First(x => x.TipNpoId == 3).Name,
                        tipNpos.First(x => x.TipNpoId == 3).Name2)
                    }
                },/*
                { "В/в", new DownClassificationLevel[] {
                    new DownClassificationLevel(13,
                        tipNpos.First(x => x.TipNpoId == 13).Name,
                        tipNpos.First(x => x.TipNpoId == 13).Name2)
                    }
                },*/
                { "Объекты закачки",
                        new DownClassificationLevel[] {
                        new DownClassificationLevel(4,
                            tipNpos.First(x => x.TipNpoId == 4).Name,
                            tipNpos.First(x => x.TipNpoId == 4).Name2),
                            new DownClassificationLevel(11,
                            tipNpos.First(x => x.TipNpoId == 11).Name,
                            tipNpos.First(x => x.TipNpoId == 11).Name2),
                            new DownClassificationLevel(14,
                            tipNpos.First(x => x.TipNpoId == 14).Name,
                            tipNpos.First(x => x.TipNpoId == 14).Name2),
                            new DownClassificationLevel(16,
                            tipNpos.First(x => x.TipNpoId == 16).Name,
                            tipNpos.First(x => x.TipNpoId == 16).Name2),
                            new DownClassificationLevel(25,
                            tipNpos.First(x => x.TipNpoId == 25).Name,
                            tipNpos.First(x => x.TipNpoId == 25).Name2),
                            new DownClassificationLevel(26,
                            tipNpos.First(x => x.TipNpoId == 26).Name,
                            tipNpos.First(x => x.TipNpoId == 26).Name2),
                            new DownClassificationLevel(27,
                            tipNpos.First(x => x.TipNpoId == 27).Name,
                            tipNpos.First(x => x.TipNpoId == 27).Name2)
                        }
                    }
                };

                _injectionInOutClassification.Add("Объекты ППД", injectionOutGroupObjects);
            }
        }

        [HttpGet("GetTipNpoTable")]
        public IActionResult GetTipNpoTable()
        {
            return Ok(db.TipNpos.ToList());
        }

        // From surface to product park
        [HttpGet("GetParentInjectionOutList")]
        public IActionResult GetParentInjectionOutList()
        {
            var result = (from scheme in db.Schemes
                          where scheme.ParentId == 0
                          select new
                          {
                              id = scheme.Id,
                              name = scheme.Nam
                          }).ToList();

            result.Insert(0, new { id = 0, name = "Не выбрано" });

            return Ok(result);
        }

        public class DownClassificationLevel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Name2 { get; set; } = "";

            DownClassificationLevel() { }

            public DownClassificationLevel(int id, string name, string name2)
            {
                Id = id;
                Name = name.Trim();
                Name2 = name2.Trim();
            }
        }

        [HttpGet("GetInjectionInOutClassification")]
        public Dictionary<string, Dictionary<string, DownClassificationLevel[]>> GetInjectionInOutClassification(bool is_injection_in_classification)
        {
            this.is_injection_out_classification = is_injection_in_classification;

            return InjectionInOutClassification;
        }

        private int[][] ConvertInOutClassificationToIdGroups()
        {
            var injectionInOutClassification = InjectionInOutClassification;

            var resultIdsGroup = injectionInOutClassification.AsParallel().AsOrdered().SelectMany(topClassification =>
                        topClassification.Value.Select(middleClassification =>
                            middleClassification.Value.Select(downClassification => downClassification.Id).ToArray()
                        )
                    ).ToArray();

            return resultIdsGroup.ToArray();
        }

        private int[] GetAcceptedTipNpoIdArray(int[] selected_npo_tip_ids)
        {
            int[][] groupedInOutClassificationIds = ConvertInOutClassificationToIdGroups();
            List<int> acceptedInt = new List<int>();

            foreach (var selected in groupedInOutClassificationIds)
            {
                if (selected.Any(id => selected_npo_tip_ids.Contains(id)))
                {
                    acceptedInt.AddRange(selected.ToList());
                }
            }

            return acceptedInt.ToArray();
        }

        class ObjectNodeFormat
        {
            public int id { get; set; }
            public int parent_id { get; set; }
            public int parent_npo_id { get; set; }
            public int tip_npo_id { get; set; }
            public string tip_npo_name { get; set; } = "";
            public string full_name { get; set; } = "";
            public string name { get; set; } = "";
        }

        // Схема сбора
        [HttpGet("GetInjectionOutTreeTable")]
        public IActionResult GetInjectionOutTreeTable(int productParkId, string selectedTipNpoIdsString)
        {
            is_injection_out_classification = true;

            var selected_tip_npo_ids = selectedTipNpoIdsString.Split(";").Select(x => Convert.ToInt32(x)).ToArray();
            var accepted_tip_npo_ids = GetAcceptedTipNpoIdArray(selected_tip_npo_ids);
            var node_edge_dictionary = new Dictionary<string, ObjectNodeFormat[]>();
            var middle_classification__nodes = new Dictionary<string, string[]>();

            // Получение всего товарного парка
            var foundTreeObjectTable = (from scheme in db.Schemes
                                        where scheme.Id == productParkId
                                        select new ObjectNodeFormat
                                        {
                                            id = scheme.Id,
                                            parent_id = scheme.ParentId,
                                            parent_npo_id = scheme.ParentTipNpoId ?? 0,
                                            tip_npo_id = scheme.TipNpoId,
                                            tip_npo_name = scheme.TipNpo.Name.Trim(),
                                            full_name = scheme.TipNpo.Name + " - " + (scheme.Nam == null ? "" : scheme.Nam.Trim()),
                                            name = scheme.Nam == null ? "" : scheme.Nam.Trim()
                                        }).ToList();

            for (int index = 0; index < foundTreeObjectTable.Count;)
            {
                var forAdd = (from scheme in db.Schemes
                              where foundTreeObjectTable.Skip(index).Select(treeObject => treeObject.id).Contains(scheme.ParentId) && (scheme.TipNpoId != 1 || scheme.TipNpoId == 1 && (scheme.FlSx == 1 || scheme.FlSx == 0))
                              select new ObjectNodeFormat
                              {
                                  id = scheme.Id,
                                  parent_id = scheme.ParentId,
                                  parent_npo_id = scheme.ParentTipNpoId ?? 0,
                                  tip_npo_id = scheme.TipNpoId,
                                  tip_npo_name = scheme.TipNpo.Name.Trim(),
                                  full_name = scheme.TipNpo.Name + " - " + (scheme.Nam == null ? "" : scheme.Nam.Trim()),
                                  name = scheme.Nam == null ? "" : scheme.Nam.Trim()
                              }).ToList();

                index = foundTreeObjectTable.Count;

                foundTreeObjectTable.AddRange(forAdd);
            }

            // Распределение и пересоединение с выбранными пунктами
            var globalClassification = InjectionInOutClassification;

            foreach (var topClassification in globalClassification.Values)
            {
                foreach (var middleClassification in topClassification)
                {
                    int[] middle_tip_npo_ids = middleClassification.Value.Select(x => x.Id).ToArray();
                    int[] selected_middle_tip_npo_ids = middle_tip_npo_ids.Where(x => selected_tip_npo_ids.Contains(x)).ToArray();

                    if (selected_middle_tip_npo_ids.Count() == 0 || middle_tip_npo_ids.Count() == 0)
                        continue;

                    var middleResult = foundTreeObjectTable.Where(x => middle_tip_npo_ids.Contains(x.tip_npo_id)).ToArray();

                    if (middleResult.Count() == 0)
                        continue;

                    string unselected_middle_tip_npo_names = string.Join(", ", middleClassification.Value.Where(x => !selected_middle_tip_npo_ids.Contains(x.Id)).Select(x => x.Name));

                    foreach (var downResult in middleResult)
                    {
                        var parent_id = downResult.parent_id;
                        var parent_npo_id = downResult.parent_npo_id;

                        // Rename an object that is not selected but is in a group where at least one other object is selected.
                        if (!selected_middle_tip_npo_ids.Contains(downResult.tip_npo_id))
                        {
                            downResult.full_name = unselected_middle_tip_npo_names + (selected_middle_tip_npo_ids.Count() + 1 == middle_tip_npo_ids.Count() ? " отсутствует" : " отсутствуют");
                        }

                        // Check parents for using in presentation
                        while (!accepted_tip_npo_ids.Contains(parent_npo_id) && parent_id != 0)
                        {
                            var parent_object = foundTreeObjectTable.First(x => x.id == parent_id);

                            parent_id = parent_object.parent_id;
                            parent_npo_id = parent_object.parent_npo_id;
                        }

                        downResult.parent_id = parent_id;
                        downResult.parent_npo_id = parent_npo_id;
                    };

                    middleResult.Reverse();

                    node_edge_dictionary.Add(middleClassification.Key, middleResult);
                    middle_classification__nodes.Add(middleClassification.Key, middleResult.Select(object_ => object_.full_name).ToArray());
                };
            }

            var topClassifications = node_edge_dictionary.Keys.Reverse().ToList();

            // Сортировка по дереву в категориях
            for (int top_class_index = 0; top_class_index < topClassifications.Count(); top_class_index++)
            {
                string top_classification_key = topClassifications[top_class_index];
                ObjectNodeFormat[] objects = node_edge_dictionary[top_classification_key];
                List<ObjectNodeFormat> sorted_objects = new List<ObjectNodeFormat>();

                if (top_class_index == 0)
                {
                    node_edge_dictionary[topClassifications[top_class_index]] = objects.OrderBy(x => x.id).ToArray();

                    continue;
                }

                int sorted_objects_count = 0;
                int top_class_parent_index = top_class_index - 1; // Для прохода по всем вышестоящим родителям

                while (sorted_objects_count < objects.Length && top_class_parent_index != -1)
                {
                    var parent_class_ids = node_edge_dictionary[topClassifications[top_class_parent_index]].Select(x => x.id).ToList(); // Можно оптимизировать заменим на проверку по parent_npo_id из другой классификации
                    var objects_for_sort_parent_ids = objects.Where(x => parent_class_ids.Contains(x.parent_id));

                    objects_for_sort_parent_ids = objects_for_sort_parent_ids.OrderBy(object_ => parent_class_ids.IndexOf(object_.parent_id));

                    sorted_objects.AddRange(objects_for_sort_parent_ids);

                    sorted_objects_count += objects_for_sort_parent_ids.Count();
                    top_class_parent_index--; // Так как родители отсортированы по убыванию дерева, то для определения родителей оставшихся объектов необъодимо идти к родителям родителей и т.д.
                }

                // Если внутри объектов есть зависимые от объектов того же уровня, например, ДНС от другого ДНС
                if (top_class_parent_index == -1)
                {
                    List<int> sorted_object_ids = sorted_objects.Select(object_ => object_.id).ToList();
                    var objects_for_inside_sorting = objects.Where(object_ => !sorted_object_ids.Contains(object_.id)).OrderBy(object_ => object_.id).ToList();

                    while (objects_for_inside_sorting.Count() > 0)
                    {
                        List<int> sorted_inside_object_ids = new List<int>();

                        foreach (var object_ in objects_for_inside_sorting)
                        {
                            int index_to_insert = sorted_object_ids.IndexOf(object_.parent_id);

                            if (index_to_insert != -1)
                            {
                                sorted_object_ids.Insert(index_to_insert + 1, object_.id);

                                sorted_objects.Insert(index_to_insert + 1, object_);

                                sorted_inside_object_ids.Add(object_.id);
                            }
                        }

                        objects_for_inside_sorting = objects_for_inside_sorting.Where(object_ => !sorted_inside_object_ids.Contains(object_.id)).ToList();
                    }
                }

                node_edge_dictionary[top_classification_key] = sorted_objects.ToArray();
            }

            kafka_client.SendMesssage(message: "Created");

            // Возрат результата
            return Ok(
                (from s in new int[1]
                 select new
                 {
                     node_edge_dictionary = node_edge_dictionary,
                     middle_classification__nodes = middle_classification__nodes
                 }).First()
           );
        }

        // Схема закачки
        [HttpGet("GetInjectionInTreeTable")]
        public IActionResult GetInjectionInTreeTable(int productParkId, string selectedTipNpoIdsString)
        {
            is_injection_out_classification = false;

            var selected_tip_npo_ids = selectedTipNpoIdsString.Split(";").Select(x => Convert.ToInt32(x)).ToArray();
            var accepted_tip_npo_ids = GetAcceptedTipNpoIdArray(selected_tip_npo_ids);
            var node_edge_dictionary = new Dictionary<string, ObjectNodeFormat[]>();
            var middle_classification__nodes = new Dictionary<string, string[]>();

            // Получение всего товарного парка
            var foundTreeObjectTable = (from scheme in db.Schemes
                                        where scheme.Id == productParkId
                                        select new ObjectNodeFormat
                                        {
                                            id = scheme.Id,
                                            parent_id = scheme.ParentId,
                                            parent_npo_id = scheme.ParentTipNpoId ?? 0,
                                            tip_npo_id = scheme.TipNpoId,
                                            tip_npo_name = scheme.TipNpo.Name.Trim(),
                                            full_name = scheme.TipNpo.Name + " - " + (scheme.Nam == null ? "" : scheme.Nam.Trim()),
                                            name = scheme.Nam == null ? "" : scheme.Nam.Trim()
                                        }).ToList();

            for (int index = 0; index < foundTreeObjectTable.Count;)
            {
                var forAdd = (from scheme in db.Schemes
                              where foundTreeObjectTable.Skip(index).Select(treeObject => treeObject.id).Contains(scheme.ParentId) && (scheme.TipNpoId != 1 || scheme.TipNpoId == 1 && (scheme.FlSx == 2 || scheme.FlSx == 0))
                              select new ObjectNodeFormat
                              {
                                  id = scheme.Id,
                                  parent_id = scheme.ParentId,
                                  parent_npo_id = scheme.ParentTipNpoId ?? 0,
                                  tip_npo_id = scheme.TipNpoId,
                                  tip_npo_name = scheme.TipNpo.Name.Trim(),
                                  full_name = scheme.TipNpo.Name + " - " + (scheme.Nam == null ? "" : scheme.Nam.Trim()),
                                  name = scheme.Nam == null ? "" : scheme.Nam.Trim()
                              }).ToList();

                index = foundTreeObjectTable.Count;

                foundTreeObjectTable.AddRange(forAdd);
            }

            // Распределение и пересоединение с выбранными пунктами
            var globalClassification = InjectionInOutClassification.Values.Reverse().ToList();

            foreach (var topClassification in globalClassification)
            {
                foreach (var middleClassification in topClassification)
                {
                    int[] middle_tip_npo_ids = middleClassification.Value.Select(x => x.Id).ToArray();
                    int[] selected_middle_tip_npo_ids = middle_tip_npo_ids.Where(x => selected_tip_npo_ids.Contains(x)).ToArray();

                    if (selected_middle_tip_npo_ids.Count() == 0 || middle_tip_npo_ids.Count() == 0)
                        continue;

                    var middleResult = foundTreeObjectTable.Where(x => middle_tip_npo_ids.Contains(x.tip_npo_id)).ToArray();

                    if (middleResult.Count() == 0)
                        continue;

                    string unselected_middle_tip_npo_names = string.Join(", ", middleClassification.Value.Where(x => !selected_middle_tip_npo_ids.Contains(x.Id)).Select(x => x.Name));

                    foreach (var downResult in middleResult)
                    {
                        var parent_id = downResult.parent_id;
                        var parent_npo_id = downResult.parent_npo_id;

                        // Rename an object that is not selected but is in a group where at least one other object is selected.
                        if (!selected_middle_tip_npo_ids.Contains(downResult.tip_npo_id))
                        {
                            downResult.full_name = unselected_middle_tip_npo_names + (selected_middle_tip_npo_ids.Count() + 1 == middle_tip_npo_ids.Count() ? " отсутствует" : " отсутствуют");
                        }

                        // Check parents for using in presentation
                        while (!accepted_tip_npo_ids.Contains(parent_npo_id) && parent_id != 0)
                        {
                            var parent_object = foundTreeObjectTable.First(x => x.id == parent_id);

                            parent_id = parent_object.parent_id;
                            parent_npo_id = parent_object.parent_npo_id;
                        }

                        downResult.parent_id = parent_id;
                        downResult.parent_npo_id = parent_npo_id;
                    };

                    middleResult.Reverse();

                    node_edge_dictionary.Add(middleClassification.Key, middleResult);
                    middle_classification__nodes.Add(middleClassification.Key, middleResult.Select(object_ => object_.full_name).ToArray());
                };
            }

            var topClassifications = node_edge_dictionary.Keys.Reverse().ToList();

            // Сортировка по дереву в категориях
            for (int top_class_index = 0; top_class_index < topClassifications.Count(); top_class_index++)
            {
                string top_classification_key = topClassifications[top_class_index];
                ObjectNodeFormat[] objects = node_edge_dictionary[top_classification_key];
                List<ObjectNodeFormat> sorted_objects = new List<ObjectNodeFormat>();

                if (top_class_index == 0)
                {
                    node_edge_dictionary[topClassifications[top_class_index]] = objects.OrderBy(x => x.id).ToArray();

                    continue;
                }

                int sorted_objects_count = 0;
                int top_class_parent_index = top_class_index - 1; // Для прохода по всем вышестоящим родителям

                while (sorted_objects_count < objects.Length && top_class_parent_index != -1)
                {
                    var parent_class_ids = node_edge_dictionary[topClassifications[top_class_parent_index]].Select(x => x.id).ToList(); // Можно оптимизировать заменим на проверку по parent_npo_id из другой классификации
                    var objects_for_sort_parent_ids = objects.Where(x => parent_class_ids.Contains(x.parent_id));

                    objects_for_sort_parent_ids = objects_for_sort_parent_ids.OrderBy(object_ => parent_class_ids.IndexOf(object_.parent_id));

                    sorted_objects.AddRange(objects_for_sort_parent_ids);

                    sorted_objects_count += objects_for_sort_parent_ids.Count();
                    top_class_parent_index--; // Так как родители отсортированы по убыванию дерева, то для определения родителей оставшихся объектов необъодимо идти к родителям родителей и т.д.
                }

                // Если внутри объектов есть зависимые от объектов того же уровня, например, ДНС от другого ДНС
                if (top_class_parent_index == -1)
                {
                    List<int> sorted_object_ids = sorted_objects.Select(object_ => object_.id).ToList();
                    var objects_for_inside_sorting = objects.Where(object_ => !sorted_object_ids.Contains(object_.id)).OrderBy(object_ => object_.id).ToList();

                    while (objects_for_inside_sorting.Count() > 0)
                    {
                        List<int> sorted_inside_object_ids = new List<int>();

                        foreach (var object_ in objects_for_inside_sorting)
                        {
                            int index_to_insert = sorted_object_ids.IndexOf(object_.parent_id);

                            if (index_to_insert != -1)
                            {
                                sorted_object_ids.Insert(index_to_insert + 1, object_.id);

                                sorted_objects.Insert(index_to_insert + 1, object_);

                                sorted_inside_object_ids.Add(object_.id);
                            }
                        }

                        objects_for_inside_sorting = objects_for_inside_sorting.Where(object_ => !sorted_inside_object_ids.Contains(object_.id)).ToList();
                    }
                }

                node_edge_dictionary[top_classification_key] = sorted_objects.ToArray();
            }

            // Возрат результата
            return Ok(
                (from s in new int[1]
                 select new
                 {
                     node_edge_dictionary = node_edge_dictionary,
                     middle_classification__nodes = middle_classification__nodes
                 }).First()
           );
        }

        [HttpGet("GetObjectInfo")]
        public IActionResult GetObjectFullInfo(int objectId)
        {
            var result = (from scheme in db.Schemes
                          join scw in db.Scws on scheme.NpoId equals scw.ScwId into scw_data
                          from scw in scw_data.DefaultIfEmpty()
                          where scheme.Id == objectId
                          select new
                          {
                              scheme.Id,
                              scheme.ParentId,
                              TipNpoName = scheme.TipNpo.Name2,
                              scheme.FlSx,
                              scheme.Ctip,
                              scheme.Nam,
                              scheme.NpoId,
                              Category = scw != null && scheme.TipNpoId == 1 ? scw.Kat.Name2 : "Нет данных",
                              State = scw != null && scheme.TipNpoId == 1 ? scw.PrSost.Name2 : "Нет данных"
                          }).FirstOrDefault();

            if (result == null)
            {
                return Ok(
                    (from s in new int[1]
                     select new
                     {
                         node_lines = new string[]{
                             $"Id: {objectId}",
                             "Fail to load"}
                     }).First());
            }

            string[] result_text = {
                    $"Id: {result.Id}",
                    $"Parent_id: {result.ParentId}",
                    $"Название: {result.Nam}",
                    $"Тип: {result.TipNpoName}",
                    $"Категория: {result.Category}\n",
                    $"Статус: {result.State}" };

            kafka_client.SendMesssage(message: "Got");

            return Ok(
                (from s in new int[1]
                 select new
                 {
                     node_lines = result_text
                 }).First());
        }
    }
}
